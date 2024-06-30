using Microsoft.UI.Xaml;
using StoryCAD.Models.Tools;
using Syncfusion.UI.Xaml.Editors;

namespace StoryCAD.Controls;

public sealed partial class Conflict
{
    public SortedDictionary<string, ConflictCategoryModel> ConflictTypes;
    private string category;
    private string subCategory;
    private ConflictCategoryModel model;
    public string ExampleText { get; set; }

    public Conflict()
    {
        InitializeComponent();
    }
    private void Category_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        category = (string)Category.Items[Category.SelectedIndex];
        model = ConflictTypes[category];
        SubCategory.ItemsSource = model.SubCategories;
        SubCategory.SelectedIndex = -1;
        Example.SelectedIndex = -1;
    }

    private void SubCategory_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        if (SubCategory.SelectedIndex > -1)
        {
            subCategory = (string)SubCategory.Items[SubCategory.SelectedIndex];
            Example.ItemsSource = model.Examples[subCategory];
        }
    }

    private void Example_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        ExampleText = (string)Example.SelectedItem;
    }

    private void Example_Loaded(object sender, RoutedEventArgs e)
    {
        ConflictTypes = Ioc.Default.GetRequiredService<ControlData>().ConflictTypes;
        Category.ItemsSource = ConflictTypes.Keys;
    }
}