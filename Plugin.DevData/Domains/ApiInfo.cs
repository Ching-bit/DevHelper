using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class ApiInfo : FileNode
{
    #region Constructors
    public ApiInfo() : base() { }

    public ApiInfo(string name, string description, IDirectoryNode? parent)
        : base(name, description, parent)
    { }
    #endregion
    
    
    #region Properties
    public int Id { get; set; }
    public List<ApiParamSet> InputParamSets { get; set; } = [];
    public List<ApiParamSet> OutputParamSets { get; set; } = [];
    public string Remark { get; set; } = string.Empty;
    #endregion


    #region Base Class
    public override bool FromFile()
    {
        try
        {
            ApiInfo fileContent = ObjectHelper.FromXmlFile<ApiInfo>(FilePath);
            Id = fileContent.Id;
            InputParamSets.Clear();
            InputParamSets.AddRange(fileContent.InputParamSets);
            OutputParamSets.Clear();
            OutputParamSets.AddRange(fileContent.OutputParamSets);
            Remark = fileContent.Remark;
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
    #endregion
}