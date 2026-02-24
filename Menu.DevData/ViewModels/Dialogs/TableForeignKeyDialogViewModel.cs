using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableForeignKeyDialogViewModel : ConfirmDialogViewModel
{
    public TableForeignKeyDialogViewModel(List<ColumnInfoModel> columnList, List<TableInfoModel> tableList, string tableName)
    {
        ColumnList = columnList;
        TableList = tableList;
        ReferenceColumnList = [];
        ForeignKeyInfoModel = new ForeignKeyInfoModel(tableName);

        ForeignKeyInfoModel.PropertyChanged -= OnForeignKeyChanged;
        ForeignKeyInfoModel.PropertyChanged += OnForeignKeyChanged;

        OnConfirmEvent -= CheckBeforeConfirm;
        OnConfirmEvent += CheckBeforeConfirm;
    }

    #region Properties
    public List<ColumnInfoModel> ColumnList { get; }
    public List<TableInfoModel> TableList { get; }
    
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _referenceColumnList;
    [ObservableProperty] private ForeignKeyInfoModel _foreignKeyInfoModel;
    #endregion
    
    private void OnForeignKeyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (nameof(ForeignKeyInfoModel.ReferenceTable) == e.PropertyName && null != ForeignKeyInfoModel.ReferenceTable)
        {
            ReferenceColumnList.Clear();
            foreach (IndexInfoModel indexInfoModel in ForeignKeyInfoModel.ReferenceTable.IndexList)
            {
                if (indexInfoModel.Type is IndexType.Primary or IndexType.Unique &&
                    1 == indexInfoModel.ColumnList.Count &&
                    !ReferenceColumnList.Select(x => x.Id).Contains(indexInfoModel.ColumnList[0].Id))
                {
                    ReferenceColumnList.Add(indexInfoModel.ColumnList[0]);
                }
            }
        }
    }

    private bool CheckBeforeConfirm()
    {
        if (null == ForeignKeyInfoModel.Column)
        {
            ShowNotification("R_STR_SELECT_EMPTY_COLUMN_NOTICE", NotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(ForeignKeyInfoModel.Name))
        {
            ShowNotification("R_STR_EMPTY_NAME_NOTICE", NotificationType.Error);
            return false;
        }

        if (null == ForeignKeyInfoModel.ReferenceTable)
        {
            ShowNotification("R_STR_SELECT_EMPTY_REFERENCE_TABLE_NOTICE", NotificationType.Error);
            return false;
        }
        
        if (null == ForeignKeyInfoModel.ReferenceColumn)
        {
            ShowNotification("R_STR_SELECT_EMPTY_REFERENCE_COLUMN_NOTICE", NotificationType.Error);
            return false;
        }

        if (!ForeignKeyInfoModel.Column.IsSameType(ForeignKeyInfoModel.ReferenceColumn))
        {
            string errMsg = ResourceHelper.FindResource<string>("R_STR_FOREIGN_KEY_COLUMN_TYPE_INCONSISTENT_NOTICE")
                .Replace("#1", ForeignKeyInfoModel.Column.GetTypeString())
                .Replace("#2", ForeignKeyInfoModel.ReferenceColumn.GetTypeString());
            ShowNotification(errMsg, NotificationType.Error);
            return false;
        }

        return true;
    }
}