using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using NLog.Fluent;
using StoryBuilder.Models;
using StoryBuilder.Services.Logging;
using StoryBuilder.Services.Messages;
using StoryBuilder.ViewModels;
using Syncfusion.UI.Xaml.Core;

namespace StoryBuilder.Controls;

public sealed partial class RelationshipView : UserControl
{
    public CharacterViewModel CharVm => Ioc.Default.GetService<CharacterViewModel>();
    public LogService _logger => Ioc.Default.GetService<LogService>();
    public RelationshipView()
    {
        InitializeComponent();
    }

    /// Instead of loading a Character's RelationshipModels directly into
    /// the ViewModel and binding them, the models themselves are loaded 
    /// into the VM's CharacterRelationships ObservableCollection, but
    /// its properties are bound only when one of of the ComboBox items
    /// CharacterRelationships is bound to is selected.
    /// However, one property need modified during LoadModel: the Partner  
    /// StoryElement in the RelationshipModel needs loaded from its Uuid.
    private void RelationshipChanged(object sender, SelectionChangedEventArgs e)
    {
        CharVm.SaveRelationship(CharVm.CurrentRelationship);
        CharVm.LoadRelationship(CharVm.SelectedRelationship);
        CharVm.CurrentRelationship = CharVm.SelectedRelationship;
    }

    /// <summary>
    /// This removes a relationship from the 'master' character.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveRelationship(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            //First identify the relationship.
            _logger.Log(LogLevel.Info, "Starting to remove relationship");
            RelationshipModel characterToDelete = null;
            foreach (var character in CharVm.CharacterRelationships)
            {   //UUID is stored in tag as a cheeky hack to identify the relationship.
                if (character.PartnerUuid == (sender as SymbolIcon).Tag) //Identify via tag.
                {
                    characterToDelete = character;
                }
            }
            _logger.Log(LogLevel.Info, $"Character to delete: {characterToDelete.Partner.Name}({characterToDelete.Partner.Uuid})");

            //Show confirmation dialog
            ContentDialogResult result = await new ContentDialog()
            {
                Title = "Are you sure?",
                Content = $"Are you sure you want to delete the relationship between {Name} and {characterToDelete.Partner.Name}?",
                XamlRoot = GlobalData.XamlRoot,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            }.ShowAsync();
            _logger.Log(LogLevel.Info, $"Dialog Result: {result}");

            if (result == ContentDialogResult.Primary) //If positive, then delete.
            {
                _logger.Log(LogLevel.Info, $"Deleting Relationship to {characterToDelete.Partner.Name}");
                Ioc.Default.GetService<CharacterViewModel>().CharacterRelationships.Remove(characterToDelete);
                _logger.Log(LogLevel.Info, $"Deleted");
                CharVm.SaveRelationships();
            }
            _logger.Log(LogLevel.Info, $"Remove relationship complete!");
        }
        catch (Exception ex)
        {
            _logger.LogException(LogLevel.Error, ex, "Error removing relationship");
        }
    }

    //When focus is lost, we save the relationship to the disk. (this is different from saving the story)
    private void LostFocus(UIElement sender, LosingFocusEventArgs args) { CharVm.SaveRelationships(); }
}