using System.Collections.ObjectModel;
using Attributes.Avalonia;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ApiParamSetModel>), "ApiParamSets")]
[WithDirectProperty(typeof(ApiParamSetType), "ParamSetType")]
[WithDirectProperty(typeof(string), "ParamSetName", "")]
[WithDirectProperty(typeof(bool), "IsParamSetChanged", false)]
[WithDirectProperty(typeof(ApiParamSetModel), "InternalSelectedParamSet", nullable: true)]
[WithDirectProperty(typeof(int), "SelectedParamSetIndex")]
public partial class ApiParamSetsPanel : UniPanel
{
    public ApiParamSetsPanel()
    {
        InitializeComponent();
        _apiParamSets = [];
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ApiParamSetsProperty)
        {
            ApiParamSets.CollectionChanged += (_, _) =>
            {
                RaisePropertyChanged(ApiParamSetsProperty!, null, ApiParamSets);
            };
        }
    }

    [RelayCommand]
    private void AddParamSet()
    {
        ApiParamSets.Add(new ApiParamSetModel(ParamSetType));
        IsParamSetChanged = true;
    }

    [RelayCommand]
    private void DeleteParamSet()
    {
        if (null == InternalSelectedParamSet)
        {
            return;
        }

        int selectedIndex = SelectedParamSetIndex;
        ApiParamSets.Remove(InternalSelectedParamSet);
        IsParamSetChanged = true;

        selectedIndex = Math.Min(selectedIndex, ApiParamSets.Count - 1);
        if (selectedIndex > 0)
        {
            InternalSelectedParamSet = ApiParamSets[selectedIndex];
        }
    }
}