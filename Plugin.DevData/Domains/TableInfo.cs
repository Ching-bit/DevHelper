using System.Text;
using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class TableInfo : FileNode
{
    #region Constructors
    public TableInfo() : base() { }

    public TableInfo(string name, string description, IDirectoryNode? parent)
        : base(name, description, parent)
    { }
    #endregion
    
    
    #region Properties
    public int Id { get; set; }
    public List<int> ColumnIdList { get; set; } = [];
    public List<IndexInfo> IndexList { get; set; } = [];
    public List<ForeignKeyInfo> ForeignKeyList { get; set; } = [];
    public bool HasHistoryTable { get; set; }
    public string Remark { get; set; } = string.Empty;
    public List<string> DefaultValues { get; set; } = [];
    #endregion
    
    
    public List<string> GetDbDefaultValues()
    {
        List<string> ret = [];
        foreach (string defaultValueString in DefaultValues)
        {
            StringBuilder sbDbDefaultValueString = new();
            string[] defaultValueArray = defaultValueString.Split(',');
            for (int i = 0; i < ColumnIdList.Count; i++)
            {
                if (sbDbDefaultValueString.Length > 0)
                {
                    sbDbDefaultValueString.Append(", ");
                }
                string defaultValue = i < defaultValueArray.Length ? defaultValueArray[i] : string.Empty;
                
                ColumnInfo? columnInfo = Global.Get<IDevData>().Columns.FirstOrDefault(x => x.Id == ColumnIdList[i]);
                if (null == columnInfo ||
                    columnInfo.Type is ColumnType.Int32 or ColumnType.Int64 or ColumnType.Number)
                {
                    sbDbDefaultValueString.Append(defaultValue);
                }
                else if (columnInfo.Type is ColumnType.Bool)
                {
                    switch (defaultValue.ToLower())
                    {
                        case "true":
                            sbDbDefaultValueString.Append('1');
                            break;
                        case "false":
                            sbDbDefaultValueString.Append('0');
                            break;
                        default:
                            sbDbDefaultValueString.Append(string.Empty);
                            break;
                    }
                }
                else
                {
                    sbDbDefaultValueString.Append($"'{defaultValue}'");
                }
            }
            
            ret.Add(sbDbDefaultValueString.ToString());
        }

        return ret;
    }
    
    
    public override bool FromFile()
    {
        try
        {
            TableInfo fileContent = ObjectHelper.FromXmlFile<TableInfo>(FilePath);
            Id = fileContent.Id;
            ColumnIdList.Clear();
            ColumnIdList.AddRange(fileContent.ColumnIdList);
            IndexList.Clear();
            IndexList.AddRange(fileContent.IndexList);
            ForeignKeyList.Clear();
            ForeignKeyList.AddRange(fileContent.ForeignKeyList);
            HasHistoryTable = fileContent.HasHistoryTable;
            Remark = fileContent.Remark;
            DefaultValues = fileContent.DefaultValues;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool ToFile()
    {
        return ObjectHelper.ToXml(FilePath, this);
    }
}