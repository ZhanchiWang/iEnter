namespace TypelessAutoEnter {
internal static class UiText {
    internal static string Language = "en";
    internal static string T(string english,string chinese) { return Language=="zh" ? chinese : english; }
}
}
