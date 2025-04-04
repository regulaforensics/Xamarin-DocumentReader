﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace DocumentReaderSample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static MainActivity Instance { get; private set; }
    public static readonly int PickImageId = 1000;
    public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);
        Instance = this;
    }
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
    {
        base.OnActivityResult(requestCode, resultCode, intent);
        if (requestCode != PickImageId) return;
        if ((resultCode == Result.Ok) && (intent != null))
        {
            Android.Net.Uri uri = intent.Data;
            Stream stream = ContentResolver.OpenInputStream(uri);

            // Set the Stream as the completion of the Task
            PickImageTaskCompletionSource.SetResult(stream);
        }
        else PickImageTaskCompletionSource.SetResult(null);
    }
}