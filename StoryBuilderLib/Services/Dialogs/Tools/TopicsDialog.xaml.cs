﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using StoryBuilder.ViewModels.Tools;

namespace StoryBuilder.Services.Dialogs.Tools;

public sealed partial class TopicsDialog
{
    public TopicsViewModel TopicsVm => Ioc.Default.GetService<TopicsViewModel>();

    public void Next_Click(object o, RoutedEventArgs routedEventArgs) { TopicsVm.NextSubTopic(); }

    public void Previous_Click(object o, RoutedEventArgs routedEventArgs) { TopicsVm.PreviousSubTopic(); }
    public TopicsDialog()
    {
        InitializeComponent();
        DataContext = TopicsVm;
    }
}