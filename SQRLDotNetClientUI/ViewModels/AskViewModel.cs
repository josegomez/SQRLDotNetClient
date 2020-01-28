﻿using ReactiveUI;
using SQRLDotNetClientUI.Views;
using SQRLUtilsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQRLDotNetClientUI.ViewModels
{
    public class AskViewModel: ViewModelBase
    {

        public Uri Site { get; set; }
        public SQRL sqrlInstance { get; set; }
        public SQRLIdentity Identity { get; set; }

        public SQRLServerResponse serverResponse { get; set; }

        public String AskMessage { get; set; } = "Hey, are you sure you want to do the thing and what not?";

        public MainWindow CurrentWindow { get; set; }

        private string askButton1 = "Button 1";
        public String AskButton1
        {
            get => askButton1;
            set => this.RaiseAndSetIfChanged(ref askButton1, value);
        }

        private string askButton2="Button 2";
        public String AskButton2
        {
            get => askButton2;
            set => this.RaiseAndSetIfChanged(ref askButton2, value);
        }

        public int ButtonValue { get; set; }
        public AskViewModel()
        {
            this.Title = "SQRL Client - Ask Protocol Question";
        }

        public AskViewModel(SQRL sqrlinstance, SQRLIdentity identity, SQRLServerResponse serverResponse)
        {
            this.Title = "SQRL Client - Ask Protocol Question";
            this.sqrlInstance = sqrlInstance;
            this.Identity = identity;
            this.serverResponse = serverResponse;
            this.AskMessage = serverResponse.AskMessage;
            this.AskButton1 = serverResponse.GetAskButtons[0];
            this.AskButton2 = serverResponse.GetAskButtons.Length>1? serverResponse.GetAskButtons[1]:string.Empty;
        }

        public void Button1()
        {
            this.ButtonValue = 1;
            this.CurrentWindow.Close(this.ButtonValue);
        }

        public void Button2()
        {
            this.ButtonValue = 2;
            this.CurrentWindow.Close(this.ButtonValue);
        }
    }
}