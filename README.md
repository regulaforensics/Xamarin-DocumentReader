# Xamarin - Regula Document Reader
Regula Document Reader SDK allows you to read various kinds of identification documents, passports, driving licenses, ID cards, etc. All processing is performed completely ***offline*** on your device. No any data leaving your device.

You can use native camera to scan the documents or image from gallery for extract all data from it.

We have provided a simple application that demonstrates the ***API*** calls you can use to interact with the DocumentReader Library.


* [How to build demo application](#how_to_build_demo_application)
* [Troubleshooting license issues](#troubleshooting_license_issues)

## <a name="how_to_build_demo_application"></a> How to build demo application
1. Get trial license for demo application at [licensing.regulaforensics.com](https://licensing.regulaforensics.com) (`regula.license` file).
1. Clone current repository using command `git clone https://github.com/regulaforensics/Xamarin-DocumentReader.git`.
1. Download and install latest [Visual Studio](https://visualstudio.microsoft.com/xamarin/) depend on your OS.
1. Copy file `regula.license` to `DocumentReaderSample/Droid/Assets` folder. 
1. Launch _Visual Studio_ and select _File -> Open_ then select _DocumentReaderSample/DocumentReaderSample.sln_ file in file browser.
1. Download additional files proposed by _Visual Studio_ to build project (build tools, for example).
1. Change application ID (at _Properties/AndroidManifest.xml_ file) for `Android` or Bundle Identifier (at _Info.plist_ file) for `iOS` to specified during registration of your license key at [licensing.regulaforensics.com](https://licensing.regulaforensics.com) (`com.regula.dr.fullrfid` by default).
1. Select appropriate build variant and run application.

## <a name="troubleshooting_license_issues"></a> Troubleshooting license issues
If you have issues with license verification when running the application, please verify that next is true:
1. OS you are using is the same as in the license you received (Android).
1. Application ID is the same that you specified for license.
1. Date and time on the device you are trying to run the application is correct and inside the license validity period.
1. You are using the latest release of the SDK.
1. You placed the license into the correct folder as described here [How to build demo application](#how_to_build_demo_application) (`DocumentReaderSample/Droid/Assets`).


# Getting Started - Android

* [How to add DocumentReader library to your project](#how_to_add_documentreader_library_to_your_project)

## <a name="how_to_add_documentreader_library_to_your_project"></a> How to add DocumentReader library to your project
To install the libraries for Android, simply open your project and install our NuGet packages named `Xamarin.DocumentReader.Api.Android` and `Xamarin.DocumentReader.Core.Full.Android`. For more details on how to install a NuGet package, [see here](https://blog.xamarin.com/xamarin-studio-and-nuget/).


Once the libraries are installed, see our [Document Reader Sample App](https://github.com/regulaforensics/Xamarin-DocumentReader/tree/master/DocumentReaderSample/Droid) for an example of how to use it.


# Getting Started - iOS
* [How to add DocumentReader library to your project](#how_to_add_documentreader_library_to_your_ios_project)
* [Initialization Core](#initialization_core)
* [Update Info.plist into your project](#update_info_plist)

## <a name="how_to_add_documentreader_library_to_your_ios_project"></a> How to add DocumentReader library to your project
To install the libraries for iOS, simply open your project and install our NuGet packages named `Xamarin.DocumentReader.Api.iOS` and `Xamarin.DocumentReader.Core.Full.iOS `. For more details on how to install a NuGet package, [see here](https://blog.xamarin.com/xamarin-studio-and-nuget/).

## <a name="initialization_core"></a> Initialization Core
After you added libraries from NuGet to the project, make sure that you initialized `Core` int the code. For example, take a look int the code below.

```c#
public partial class ViewController : UIViewController
{
   ....
   
   protected ViewController(IntPtr handle) : base(handle) =>
        //WARNING!!!!
        //Initialization DocumentReader from DocReaderCore is required
        new DocReaderCore.iOS.DocumentReader();
   
   ....
}
```

FYI: Xamarin is not included into ipa frameworks which are not called in the code. You can successfylly build project and run, but in runtime you will get error that DocumentReaderCore library not loaded. See error below.

`dyld: Library not loaded: @rpath/DocumentReaderCore.framework/DocumentReaderCore`

## <a name="update_info_plist"></a>Update Info.plist into your project
Don't forgot update your plist with next properties:
* Privacy - Camera Usage Description
* Privacy - Photo Library Usage Description

It's required for make a snapshot from camera or get a photo from gallery.

Once the libraries are installed, see our [Document Reader Sample App](https://github.com/regulaforensics/Xamarin-DocumentReader/tree/master/DocumentReaderSample/iOS) for an example of how to use it.
