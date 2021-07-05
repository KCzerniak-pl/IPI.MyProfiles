using MyProfiles.Models;
using System;
using System.IO;

public static class Common
{
    // Ścieżka do folderu, w którym zapisane są profile (w lokalizacji, w której działa program).
    public static string PathDirProfiles { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles");



    // Ścieżka do wybranego proflu.
    public static string PathFileProfile(string guidString) { return Path.Combine(PathDirProfiles, $"{guidString}.json"); }



    // Przechowuje wybrany profil.
    public static ProfileModel SelectedProfile { get; set; }
}