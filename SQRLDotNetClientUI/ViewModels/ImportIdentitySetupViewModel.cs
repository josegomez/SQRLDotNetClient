﻿
using ReactiveUI;
using SQRLDotNetClientUI.Models;
using SQRLDotNetClientUI.Views;
using SQRLUtilsLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQRLDotNetClientUI.ViewModels
{ 
    public class ImportIdentitySetupViewModel : ViewModelBase
    {
        public SQRLIdentity Identity { get; set; }
        public string IdentityName { get; set; } = "";
        public string RescueCode { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordVerify { get; set; }

        public ImportIdentitySetupViewModel()
        {
            Init();
        }
        public ImportIdentitySetupViewModel(SQRLIdentity identity)
        {
            Init();
            this.Identity = identity;
        }

        private void Init()
        {
            this.Title = _loc.GetLocalizationValue("ImportIdentitySetupWindowTitle");
        }

        public void Previous()
        {
            ((MainWindowViewModel)_mainWindow.DataContext).Content = 
                ((MainWindowViewModel)_mainWindow.DataContext).MainMenu;
        }

        public void Cancel()
        {
            ((MainWindowViewModel)_mainWindow.DataContext).Content =
                ((MainWindowViewModel)_mainWindow.DataContext).MainMenu;
        }

        public async void VerifyAndImportIdentity()
        {
            var progressBlock1 = new Progress<KeyValuePair<int, string>>();
            var progressBlock2 = new Progress<KeyValuePair<int, string>>();
            var progressDecryptBlock2 = new Progress<KeyValuePair<int, string>>();

            var progressDialog = new ProgressDialog(new List<Progress<KeyValuePair<int, string>>>() { 
                progressBlock1, progressBlock2, progressDecryptBlock2 });
            _ = progressDialog.ShowDialog(_mainWindow);

            var iukData = await SQRL.DecryptBlock2(
                this.Identity, SQRL.CleanUpRescueCode(this.RescueCode), progressDecryptBlock2);

            if (iukData.DecryptionSucceeded)
            {
                SQRLIdentity newId = this.Identity.Clone();
                byte[] imk = SQRL.CreateIMK(iukData.Iuk);

                if (!newId.HasBlock(0)) SQRL.GenerateIdentityBlock0(imk, newId);
                var block1 = SQRL.GenerateIdentityBlock1(iukData.Iuk, this.NewPassword, newId, progressBlock1);
                var block2 = SQRL.GenerateIdentityBlock2(iukData.Iuk, SQRL.CleanUpRescueCode(this.RescueCode), newId, progressBlock2);
                await Task.WhenAll(block1, block2);

                progressDialog.Close();

                if (newId.HasBlock(3)) SQRL.GenerateIdentityBlock3(iukData.Iuk, this.Identity, newId, imk, imk); 

                newId.IdentityName = this.IdentityName;

                try
                {
                    _identityManager.ImportIdentity(newId, true);
                }
                catch (InvalidOperationException e)
                {
                    var btnRsult = await new Views.MessageBox(
                        _loc.GetLocalizationValue("ErrorTitleGeneric"), e.Message,
                        MessageBoxSize.Medium, MessageBoxButtons.OK, MessageBoxIcons.ERROR)
                        .ShowDialog<MessagBoxDialogResult>(_mainWindow);
                }
                finally
                {
                    ((MainWindowViewModel)_mainWindow.DataContext).Content =
                    ((MainWindowViewModel)_mainWindow.DataContext).MainMenu;
                }
            }
            else
            {
                progressDialog.Close();

                var btnRsult = await new Views.MessageBox(
                    _loc.GetLocalizationValue("ErrorTitleGeneric"),
                    _loc.GetLocalizationValue("InvalidRescueCodeMessage"),
                    MessageBoxSize.Medium, MessageBoxButtons.OK, MessageBoxIcons.ERROR)
                    .ShowDialog<MessagBoxDialogResult>(_mainWindow);
            }
        }
    }
}
