// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace DocumentReaderSingleSample.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton bntCamera { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView documentImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView initIndocator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel initLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel nameLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView portraitImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel rfidLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch rfidSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView scenariosView { get; set; }

        [Action ("UseCameraButtonTouch:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UseCameraButtonTouch (UIKit.UIButton sender);

        [Action ("UseGaleryButtonTouch:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UseGaleryButtonTouch (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (bntCamera != null) {
                bntCamera.Dispose ();
                bntCamera = null;
            }

            if (btnImage != null) {
                btnImage.Dispose ();
                btnImage = null;
            }

            if (documentImage != null) {
                documentImage.Dispose ();
                documentImage = null;
            }

            if (initIndocator != null) {
                initIndocator.Dispose ();
                initIndocator = null;
            }

            if (initLabel != null) {
                initLabel.Dispose ();
                initLabel = null;
            }

            if (nameLbl != null) {
                nameLbl.Dispose ();
                nameLbl = null;
            }

            if (portraitImageView != null) {
                portraitImageView.Dispose ();
                portraitImageView = null;
            }

            if (rfidLabel != null) {
                rfidLabel.Dispose ();
                rfidLabel = null;
            }

            if (rfidSwitch != null) {
                rfidSwitch.Dispose ();
                rfidSwitch = null;
            }

            if (scenariosView != null) {
                scenariosView.Dispose ();
                scenariosView = null;
            }
        }
    }
}