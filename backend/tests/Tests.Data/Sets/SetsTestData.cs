using Domain.Sets;

namespace Tests.Data.Sets;

public static class SetsTestData
{
    public static Set FirstTestSet() => Set.New("English Vocabulary", "Common English words", SetType.Language, "en", "uk", true, false);
    public static Set SecondTestSet() => Set.New("German Grammar", "German grammar rules", SetType.Language, "de", "uk", true, false);
    public static Set ThirdTestSet() => Set.New("History Dates", "World history dates", SetType.Subject, "uk", "uk", false, false);
}
