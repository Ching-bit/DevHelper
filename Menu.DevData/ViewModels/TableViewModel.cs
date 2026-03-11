using System.Text;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Control.Basic;
using Framework.Common;
using Framework.Utils;
using Plugin.DevData;
using Plugin.DevData.Messages;

namespace Menu.DevData;

public partial class TableViewModel : UniViewModel
{
    #region Menu Lifecycle
    public override void OnMenuInit()
    {
        base.OnMenuInit();
        InitData();
        
        // register language changed
        WeakReferenceMessenger.Default.Register<ColumnsChangedMessage>(this, (_, message) =>
        {
            if (null == TableInfoModel) { return; }
            
            List<ColumnInfo> columns = message.Value;
            foreach (ColumnInfoModel columnInfoModel in TableInfoModel.ColumnList)
            {
                ColumnInfo? columnInfo = columns.FirstOrDefault(x => x.Id == columnInfoModel.Id);
                if (null == columnInfo) { continue; }
                columnInfoModel.CopyFromColumnInfo(columnInfo);
            }
        });
    }
    
    private void InitData()
    {
        if (View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not TableInfo tableInfo)
        {
            throw new Exception("Cannot get table instance from the menu");
        }
        
        TableInfoModel = new TableInfoModel(tableInfo);
        IsTableChanged = false;
    }
    #endregion
    
    
    #region Commands
    [RelayCommand]
    private async Task Save()
    {
        if (null == TableInfoModel || View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not TableInfo tableInfo)
        {
            throw new Exception("Cannot get table instance from the menu");
        }
        
        if (!await CheckBeforeSave(TableInfoModel))
        {
            return;
        }

        foreach (ColumnInfoModel deletedColumn in TableInfoModel.ColumnList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList())
        {
            TableInfoModel.ColumnList.Remove(deletedColumn);
        }
        
        foreach (IndexInfoModel deletedIndex in TableInfoModel.IndexList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList())
        {
            TableInfoModel.IndexList.Remove(deletedIndex);
        }
        
        foreach (ForeignKeyInfoModel deletedForeignKey in TableInfoModel.ForeignKeyList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList())
        {
            TableInfoModel.ForeignKeyList.Remove(deletedForeignKey);
        }

        List<string> defaultValues = [];
        foreach (DynamicRow defaultValue in TableInfoModel.DefaultValues)
        {
            StringBuilder sbDefaultValue = new();
            foreach (ColumnInfoModel columnInfoModel in TableInfoModel.ColumnList)
            {
                if (sbDefaultValue.Length > 0)
                {
                    sbDefaultValue.Append(',');
                }
                sbDefaultValue.Append(defaultValue[columnInfoModel.Name]);
            }
            defaultValues.Add(sbDefaultValue.ToString());
        }

        if (!Global.Get<IDevData>().UpdateTable(tableInfo,
                TableInfoModel.ColumnList.Select(x => x.Id).ToList(),
                TableInfoModel.IndexList.Select(x => x.GetIndexInfo()).ToList(),
                TableInfoModel.ForeignKeyList.Select(x => x.GetForeignKeyInfo()).ToList(),
                TableInfoModel.HasHistoryTable,
                TableInfoModel.Remark,
                defaultValues))
        {
            ShowNotification("R_STR_SAVE_FAILED", NotificationType.Error);
            return;
        }

        InitData();
    }

    private async Task<bool> CheckBeforeSave(TableInfoModel tableInfoModel)
    {
        // check if the column exists
        foreach (ColumnInfoModel columnInfoModel in tableInfoModel.ColumnList.Where(x => ModifyStatus.Deleted != x.ModifyStatus))
        {
            if (Global.Get<IDevData>().Columns.All(x => x.Id != columnInfoModel.Id))
            {
                string errMsg = ResourceHelper.FindResource<string>("R_STR_COLUMN_NOT_FOUND_NOTICE")
                    .Replace("#1", $"[{columnInfoModel}]");
                await MessageDialog.Show(errMsg);
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void Restore()
    {
        InitData();
    }
    #endregion
    
    
    #region Properties
    [ObservableProperty] private TableInfoModel? _tableInfoModel;
    [ObservableProperty] private bool _isTableChanged;
    #endregion
    
}