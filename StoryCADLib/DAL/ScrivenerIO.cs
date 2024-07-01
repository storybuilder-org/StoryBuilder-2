﻿using Windows.Data.Xml.Dom;
using Windows.Storage;
using NRtfTree.Core;
using StoryCAD.Models.Scrivener;

namespace StoryCAD.DAL;

/// <summary>
/// A data source that provides methods to read, parse, 
/// and update a Scrivener project. 
/// </summary>
public class ScrivenerIo
{
    // The .scrivener document
    public XmlDocument XmlDocument;
    public StorageFile ScrivenerFile;
    public string ProjectPath;
    public IXmlNode ScrivenerProject;
    public IXmlNode Binder;
    public IXmlNode StoryCAD;
    public IXmlNode Research;
    public IXmlNode LabelSettings;
    public IXmlNode StatusSettings;
    public IXmlNode CustomMetaDataSettings;
    public IXmlNode StbUuidSetting;
    public IXmlNode ProjectBookMarks;


    /// <summary>
    /// Create a BinderItem tree from a .scrivx Scrivener project file.
    ///
    /// This method builds the tree via recursive descent through
    /// the XML.
    ///
    /// Note that this is not currently a complete parse of the
    /// Scrivener project file. It focuses on what's needed to add
    /// or replace a StoryCAD project into the file.
    /// </summary>
    /// <returns>model's root BinderItem</returns>
    public async Task LoadScrivenerProject()
    {
        XmlDocument = await XmlDocument.LoadFromFileAsync(ScrivenerFile);
        ScrivenerProject = XmlDocument.SelectSingleNode("ScrivenerProject");
        Binder = XmlDocument.SelectSingleNode("ScrivenerProject//Binder");
        StoryCAD = XmlDocument.SelectSingleNode("ScrivenerProject//Binder//BinderItem[Title='StoryCAD']");
        Research = XmlDocument.SelectSingleNode("ScrivenerProject//Binder//BinderItem[Title='Research']");
        LabelSettings = XmlDocument.SelectSingleNode("ScrivenerProject//LabelSettings");
        StatusSettings = XmlDocument.SelectSingleNode("ScrivenerProject//StatusSettings");
        CustomMetaDataSettings = XmlDocument.SelectSingleNode("ScrivenerProject//CustomMetaDataSettings");
        StbUuidSetting = XmlDocument.SelectSingleNode("ScrivenerProject//CustomMetaDataSettings//MetaDataField[Title='stbuuid']");
        ProjectBookMarks = XmlDocument.SelectSingleNode("ScrivenerProject//ProjectBookmarks");
    }

    public async Task SaveScrivenerProject(StorageFile file)
    {
        await XmlDocument.SaveToFileAsync(file);
    }

    public BinderItem BuildBinderItemTree()
    {
        BinderItem _root = new("", BinderItemType.Root, "Root");
        RecurseXmlNode(Binder, _root);
        return _root;
    }

    /// <summary>
    /// Parse a node in a Scrivener project file, adding it to
    /// its parent, and then parse the node's children recursively.
    ///
    /// Called from LoadBinderFromScrivener().
    /// </summary>
    /// <param name="node">The XmlElement to parse</param>
    /// <param name="parent">node's parent BinderItem</param>
    private void RecurseXmlNode(IXmlNode node, BinderItem parent)
    {
        if (node.NodeType != NodeType.ElementNode)
            return;

        string _uuid = string.Empty;
        string _created = string.Empty;
        string _modified = string.Empty;
        BinderItemType _type = BinderItemType.Unknown;

        if (node.NodeName.Equals("Binder"))
        {
            foreach (IXmlNode _node in node.ChildNodes) { RecurseXmlNode(_node, parent); }
        }
        else if (node.NodeName.Equals("BinderItem"))
        {
            // Search for the attributes we want on a BindItem.
            foreach (IXmlNode _attr in node.Attributes)
            {
                switch (_attr.NodeName)
                {
                    case "UUID":
                        _uuid = _attr.InnerText;
                        break;
                    case "Created":
                        _created = _attr.InnerText;
                        break;
                    case "Modified":
                        _modified = _attr.InnerText;
                        break;
                    case "Type":
                        switch (_attr.InnerText)
                        {
                            case "Text":
                                _type = BinderItemType.Text;
                                break;
                            case "Folder":
                                _type = BinderItemType.Folder;
                                break;
                            case "DraftFolder":
                                _type = BinderItemType.DraftFolder;
                                break;
                            case "ResearchFolder":
                                _type = BinderItemType.ResearchFolder;
                                break;
                            case "TrashFolder":
                                _type = BinderItemType.TrashFolder;
                                break;
                            case "PDF":
                                _type = BinderItemType.Pdf;
                                break;
                            case "WebArchive":
                                _type = BinderItemType.WebArchive;
                                break;
                            case "Root":
                                _type = BinderItemType.Root;
                                break;
                        }
                        break;
                }
            }
            IXmlNode _titleNode = node.SelectSingleNode("./Title");
            string _title = _titleNode != null ? _titleNode.InnerText : string.Empty;
            IXmlNode _children = node.SelectSingleNode("./Children");
            // Process stbuuid
            IXmlNode _metaData = node.SelectSingleNode("./MetaData");
            if (_metaData != null)
            {
                IXmlNode _customMetaData = _metaData.SelectSingleNode("./CustomMetaData");
                XmlElement _stbUuidNode = null;
                if (_customMetaData != null)
                    _stbUuidNode = (XmlElement)_customMetaData.SelectSingleNode("./MetaDataItem[FieldID='stbuuid']");
                string _stbUuid = string.Empty;
                if (_stbUuidNode != null)
                {
                    XmlElement _value = (XmlElement)_stbUuidNode.SelectSingleNode("./Value");
                    if (_value != null) _stbUuid = _value.InnerText;
                }
                BinderItem _newNode = new(_uuid, _type, _title, parent, _created, _modified, _stbUuid) { Node = node };
                if (_children != null)
                {
                    foreach (IXmlNode _child in _children.ChildNodes)
                        if (_child.NodeName.Equals("BinderItem")) { RecurseXmlNode(_child, _newNode); }
                }
            }
        }
    }

    /// <summary>
    /// Create an XmlElement tree or subtree from a BinderItem model.
    ///
    /// This method builds the tree via recursive descent through
    /// the BinderItem model.
    /// </summary>
    /// <param name="rootItem"></param>
    /// <returns></returns>
    public XmlElement CreateFromBinder(BinderItem rootItem)
    {
        XmlElement _xmlRootElement = CreateNode(rootItem);
        RecurseBinderNode(rootItem, _xmlRootElement);

        return _xmlRootElement;
    }

    private void RecurseBinderNode(BinderItem binderNode, IXmlNode xmlParentNode)
    {
        IXmlNode _parentChildren = xmlParentNode.SelectSingleNode(".//Children");
        // Traverse and create XmlNodes for the binderNode subtree
        foreach (BinderItem _child in binderNode.Children)
        {
            IXmlNode _newNode = CreateNode(_child);
            if (_parentChildren != null) {_parentChildren.AppendChild(_newNode);}
            //xmlParentNode.AppendChild(newNode);
            RecurseBinderNode(_child, _newNode);
        }
    }

    /// <summary>
    /// Create a new XmlElement corresponding to a BinderItem node.
    /// </summary>
    /// <param name="binderItem"></param>
    /// <returns></returns>
    public XmlElement CreateNode(BinderItem binderItem)
    {
        // Create new XmlElement for BinderItem
        XmlElement _element = XmlDocument.CreateElement("BinderItem");
        // Set attributes
        _element.SetAttribute("UUID", binderItem.Uuid);
        _element.SetAttribute("Type", GetType(binderItem.Type));
        DateTime _now = DateTime.Now;
        _element.SetAttribute("Created", _now.ToString("yyyy-MM-dd - HH:mm:ss - K"));
        _element.SetAttribute("Modified", _now.ToString("yyyy-MM-dd - HH:mm:ss - K"));
        // Add Title child
        XmlElement _title = XmlDocument.CreateElement("Title");
        XmlText _titleText = XmlDocument.CreateTextNode(binderItem.Title);
        _title.AppendChild(_titleText);
        _element.AppendChild(_title);
        // Add MetaData child and IncludeInCompile grandchild
        XmlElement _meta = XmlDocument.CreateElement("MetaData");
        XmlElement _include = XmlDocument.CreateElement("IncludeInCompile");
        XmlText _includeText = XmlDocument.CreateTextNode("Yes");
        _include.AppendChild(_includeText);
        _meta.AppendChild(_include);
        _element.AppendChild(_meta);
        // Add TextSettings child and TextSelection grandchild
        XmlElement _textSettings = XmlDocument.CreateElement("TextSettings");
        XmlElement _textSelection = XmlDocument.CreateElement("TextSelection");
        XmlText _textSelectionText = XmlDocument.CreateTextNode("0,0");
        _textSelection.AppendChild(_textSelectionText);
        _textSettings.AppendChild(_textSelection);
        _element.AppendChild(_textSelection);
        // Note: In Scrivener text nodes can have sub-documents and folders
        // can have their own text.
        XmlElement _children = XmlDocument.CreateElement("Children");
        _element.AppendChild(_children);
        //if (binderItem.Type.Equals((BinderItemType.Folder)))
        //{
        //    XmlElement children = XmlDocument.CreateElement("Children");
        //    element.AppendChild(children);
        //}
        binderItem.Node = _element;
        return _element;
    }

    /// <summary>
    /// Return a string representation of a BinderItemType enum.
    /// </summary>
    /// <param name="type">The enum to parse</param>
    /// <returns>A string representation of the enum value.</returns>
    private static string GetType(BinderItemType type)
    {
        //TODO: Handle Unknown and default cases in switch (and hence remove the following Resharper ignore commend)
        string _value = string.Empty;
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type)
        {
            case BinderItemType.Text:
                _value = "Text";
                break;
            case BinderItemType.Folder:
                _value = "Folder";
                break;
            case BinderItemType.DraftFolder:
                _value = "DraftFolder";
                break;
            case BinderItemType.ResearchFolder:
                _value = "ResearchFolder";
                break;
            case BinderItemType.TrashFolder:
                _value = "TrashFolder";
                break;
            case BinderItemType.Pdf:
                _value = "PDF";
                break;
            case BinderItemType.WebArchive:
                _value = "WebArchive";
                break;
            case BinderItemType.Root:
                _value = "Root";
                break;
        }
        return _value;
    }

    public async Task<bool> IsScrivenerRelease3()
    {
        string _path = Path.Combine(ProjectPath, "Files");
        StorageFolder _filesFolder = await StorageFolder.GetFolderFromPathAsync(_path);
        IReadOnlyList<StorageFolder> _folders = await _filesFolder.GetFoldersAsync();
        foreach (StorageFolder _folder in _folders)
            if (_folder.Name.Equals("Data"))
                return true;    // The project is a Scrivener Release 3 folder
        return false;
    }

    /// <summary>
    /// Get the Scrivener document subfolder for a UUID.
    /// 
    /// If the folder already exists, return it.
    /// If the subfolder doesn't exist, create and return it.
    /// </summary>
    /// <param name="uuid">The UUID of a StoryCAD StoryElement or list</param>
    /// <returns>StorageFolder</returns>
    public async Task<StorageFolder> GetSubFolder(string uuid)
    {
        string _path = Path.Combine(ProjectPath, "Files", "Data");
        StorageFolder _parent = await StorageFolder.GetFolderFromPathAsync(_path);

        try
        {
            return await _parent.GetFolderAsync(uuid);
        }
        catch (Exception)
        {
            return await _parent.CreateFolderAsync(uuid);
        }
    }

    public ScrivenerIo()
    {
        XmlDocument = new XmlDocument();
        ProjectPath = string.Empty;
    }

    public async Task<string> ReadRtfText(string path)
    {
        string _result = string.Empty;
        await Task.Run(() =>
        {
            //Create an RTF object
            RtfTree _tree = new();

            //Load an RTF document from a file
            _tree.LoadRtfFile(path);

            // Get the file's contents
            return _tree.Text;
        });
        return _result;
    }

    /// <summary>
    /// Write a Xml file or subtree to disk.
    ///
    /// This routine is intended as a debugging aid. 
    /// </summary>
    public async Task WriteTestFile(string fileName, XmlElement root)
    {
        StorageFolder _projectFolder = await StorageFolder.GetFolderFromPathAsync(ProjectPath);
        StorageFile _file = await _projectFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        if (root != null)
        {
            XmlDocument _doc = new();
            _doc.AppendChild(_doc.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\""));
            XmlElement _newRoot = _doc.CreateElement("Binder");
            _doc.AppendChild(_newRoot);
            IXmlNode _newNode = _doc.ImportNode(root, true);
            _newRoot.AppendChild(_newNode);
            await _doc.SaveToFileAsync(_file);
        }
    }
}