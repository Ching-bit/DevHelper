using System.Collections.ObjectModel;
using Attributes.Avalonia;
using Avalonia;
using CommunityToolkit.Mvvm.Messaging;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

[WithDirectProperty(typeof(ApiParamSetModel), "ApiParamSetModel", nullable: true)]
[WithDirectProperty(typeof(bool), "IsParamSetChanged", false)]
[WithDirectProperty(typeof(List<string>), "InternalParameterSetModeList")]
[WithDirectProperty(typeof(ObservableCollection<TableInfoModel>), "InternalTableList")]
public partial class ApiSingleParamSetPanel : UniPanel
{
    public ApiSingleParamSetPanel()
    {
        InitializeComponent();
        _internalParameterSetModeList = [];
        _internalTableList = [];
        
        RefreshParameterSetModeList();
        RefreshTableList();

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (_, _) =>
        {
            RefreshParameterSetModeList();
        });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ApiParamSetModelProperty && null != ApiParamSetModel)
        {
            ApiParamSetModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ApiParamSetModel.IsParamSetChanged))
                {
                    IsParamSetChanged = ApiParamSetModel.IsParamSetChanged;
                }
            };
        }
    }

    private void RefreshParameterSetModeList()
    {
        InternalParameterSetModeList.AddRange([
            $"0 {ResourceHelper.FindResource<string>("R_STR_SELF_DEFINED", "Self Defined")}",
            $"1 {ResourceHelper.FindResource<string>("R_STR_ASSOCIATE_TABLE", "Associate Table")}"
        ]);
    }

    private void RefreshTableList()
    {
        InternalTableList.Clear();
        foreach (TableInfo tableInfo in Global.Get<IDevData>().GetAllTables().Values.SelectMany(list => list))
        {
            InternalTableList.Add(new TableInfoModel(tableInfo));
        }
    }

}