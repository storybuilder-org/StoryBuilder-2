﻿namespace StoryCAD.Views;

public sealed partial class TrashCanPage
{
    public TrashCanViewModel TrashCanVm => Ioc.Default.GetService<TrashCanViewModel>();
    public TrashCanPage()
    {
        InitializeComponent();
        DataContext = TrashCanVm;
    }
}