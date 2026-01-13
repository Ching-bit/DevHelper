using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;

namespace Menu.DevData;

public partial class DataDictionaryDialogViewModel : ConfirmDialogViewModel
{
    public DataDictionaryDialogViewModel(string dataDict)
    {
        List<DataDictionaryItemModel> dictItems = StringToDict(dataDict);
        foreach (DataDictionaryItemModel dataDictionaryItemModel in dictItems)
        {
            DataDictionaryItems.Add(dataDictionaryItemModel);
        }
    }
    
    [ObservableProperty] private ObservableCollection<DataDictionaryItemModel> _dataDictionaryItems = [];

    [RelayCommand]
    private void Add()
    {
        DataDictionaryItems.Add(new DataDictionaryItemModel());
    }

    [RelayCommand]
    private void Delete()
    {
        DataGrid? dataGrid = View?.FindControl<DataGrid>("DataGridDataDictionary");
        if (null == dataGrid) { return; }

        List<DataDictionaryItemModel> deletedItems = [];
        deletedItems.AddRange(dataGrid.SelectedItems.OfType<DataDictionaryItemModel>());
        foreach (DataDictionaryItemModel deletedItem in deletedItems)
        {
            DataDictionaryItems.Remove(deletedItem);
        }
    }
    
    private static List<DataDictionaryItemModel> StringToDict(string dataDict)
    {
        List<DataDictionaryItemModel> ret = [];
        string[] valueMeaningPairs = dataDict.Split(';');
        foreach (string valueMeaningPair in valueMeaningPairs)
        {
            string[] valueAndMeaning = valueMeaningPair.Split(":");
            if (2 != valueAndMeaning.Length)
            {
                continue;
            }
            ret.Add(new DataDictionaryItemModel
            {
                Value = valueAndMeaning[0].Trim(),
                Meaning = valueAndMeaning[1].Trim()
            });
        }
        
        return ret;
    }

    public static string DictToString(List<DataDictionaryItemModel> dict)
    {
        StringBuilder sb = new();
        foreach (DataDictionaryItemModel dataDictionaryItemModel in dict)
        {
            if (string.IsNullOrEmpty(dataDictionaryItemModel.Value))
            {
                continue;
            }
            
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }

            sb.Append($"{dataDictionaryItemModel.Value}: {dataDictionaryItemModel.Meaning}");
        }
        
        return sb.ToString();
    }
}