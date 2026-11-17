using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace TypelessAutoEnter {
[DataContract] internal sealed class Settings {
    [DataMember] public bool Enabled = false;
    [DataMember] public string Mode = "always";
    [DataMember] public int DelayMs = 500;
    [DataMember] public bool AllApps = false;
    [DataMember] public string AllowedApps = "Codex";
    [DataMember] public string EndWord = "over";
    [DataMember] public string Language = "en";
    internal static readonly string DirectoryPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AutoEnter");
    internal static Settings Load() {
        try {
            var path=Path.Combine(DirectoryPath,"settings.json");
            if(!File.Exists(path)) path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"TypelessAutoEnter","settings.json");
            using(var f=File.OpenRead(path)) {
            var s=(Settings)new DataContractJsonSerializer(typeof(Settings)).ReadObject(f); s.Normalize();
            return s;
        } } catch { return new Settings(); }
    }
    internal void Normalize() {
        if(Mode!="over") Mode="always";
        DelayMs=Math.Max(300,Math.Min(3000,DelayMs));
        AllowedApps=AllowedApps ?? "Codex";
        // Older settings have no EndWord member. An explicitly empty value is
        // different: it disables conditional sending until the user enters a word.
        EndWord=EndWord==null ? "over" : EndWord.Trim();
        if(Language!="zh") Language="en";
    }
    internal void Save() {
        Directory.CreateDirectory(DirectoryPath);
        var path=Path.Combine(DirectoryPath,"settings.json");
        using(var f=File.Create(path+".tmp")) new DataContractJsonSerializer(typeof(Settings)).WriteObject(f,this);
        if(File.Exists(path)) File.Replace(path+".tmp",path,null); else File.Move(path+".tmp",path);
    }
}
internal static class Rules {
    private static string cachedEndWord;
    private static Regex endWordPattern;
    internal static bool IsTypelessPaste(uint vk, uint flags, ulong extra, bool control) {
        return vk==0x56 && (flags&0x10)!=0 && extra==Native.TypelessTag && control;
    }
    internal static bool IsValidEndWord(string word) {
        if(String.IsNullOrWhiteSpace(word) || word.Trim().Length>64) return false;
        foreach(char c in word) if(Char.IsControl(c)) return false;
        return true;
    }
    internal static bool TryStripEndWord(string text,string word,out string clean) {
        clean=text;
        if(String.IsNullOrEmpty(text) || !IsValidEndWord(word)) return false;
        word=word.Trim();
        if(word!=cachedEndWord) {
            char first=word[0];
            bool latin=(first>='A' && first<='Z') || (first>='a' && first<='z') || (first>='0' && first<='9') || first=='_';
            // Match the user's literal phrase, never a user-provided regex.
            // Chinese can directly follow Chinese; English must not match inside a word.
            endWordPattern=new Regex((latin ? @"(?<![A-Za-z0-9_])" : "")+Regex.Escape(word)+@"[\s.!?。！？…，,;；:：]*\z",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
            cachedEndWord=word;
        }
        var m=endWordPattern.Match(text); if(!m.Success) return false;
        var body=text.Substring(0,m.Index).TrimEnd(' ','\t','\r','\n',',','，',';','；',':','：');
        if(String.IsNullOrWhiteSpace(body)) return false;
        clean=body; return true;
    }
    internal static bool Allows(Settings s,string process) {
        if(String.IsNullOrEmpty(process)) return false;
        if(s.AllApps) return true;
        foreach(var a in s.AllowedApps.Split(new[]{',',';','，','；','\n','\r'},StringSplitOptions.RemoveEmptyEntries)) {
            string p=a.Trim(); if(p.EndsWith(".exe",StringComparison.OrdinalIgnoreCase)) p=p.Substring(0,p.Length-4);
            if(String.Equals(p,process,StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}
internal sealed class Pending {
    public IntPtr Window, Focus;
    public long Due, Expires;
    public int Generation;
    public bool CanFire(long now,IntPtr window,IntPtr focus,int generation,bool enabled,bool modifiers) {
        return enabled && window==Window && focus==Focus && generation==Generation && now>=Due && now<=Expires && !modifiers;
    }
}
}
