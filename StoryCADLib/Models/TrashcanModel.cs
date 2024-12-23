﻿using Windows.Data.Xml.Dom;

namespace StoryCAD.Models;

/// <summary>
/// The TrashCanModel is a container for deleted StoryElements. It's the second root node
/// in both the Explorer View and Narrator Views, and contains no properties.
/// </summary>
public class TrashCanModel : StoryElement
{
    #region Constructors
    public TrashCanModel(StoryModel model) : base("Deleted Story Elements", StoryItemType.TrashCan, model) { }
    
    //TODO: REMOVE WITH STORYREADER
    public TrashCanModel(IXmlNode xn, StoryModel model) : base(xn, model) { }

    #endregion
}