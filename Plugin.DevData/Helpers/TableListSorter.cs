namespace Plugin.DevData.Helpers;

public class TableListSorter
{
    public static List<TableInfo> Sort(List<TableInfo> nodes)
    {
        Dictionary<TableInfo, int> indegree = [];
        foreach (TableInfo node in nodes)
        {
            indegree.TryAdd(node, 0);

            foreach (TableInfo? refNode in node.ForeignKeyList.Select(x => nodes.FirstOrDefault(t => t.Id == x.TableId)))
            {
                if (null == refNode) { continue; }

                indegree.TryAdd(node, 0);
                indegree[node]++;
            }
        }
        
        SortedSet<TableInfo> queue = new(Comparer<TableInfo>.Create((a, b) =>
        {
            int cmp = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            return cmp != 0 ? cmp : a.GetHashCode().CompareTo(b.GetHashCode());
        }));

        foreach (KeyValuePair<TableInfo, int> kv in indegree.Where(kv => kv.Value == 0))
        {
            queue.Add(kv.Key);
        }

        List<TableInfo> result = [];

        while (queue.Count > 0)
        {
            TableInfo node = queue.Min!;
            queue.Remove(node);
            result.Add(node);

            foreach (TableInfo? refNode in node.ForeignKeyList.Select(x => nodes.FirstOrDefault(t => t.Id == x.TableId)))
            {
                if (null == refNode) { continue; }
                
                indegree[refNode]--;
                if (indegree[refNode] == 0)
                {
                    queue.Add(refNode);
                }
            }
        }
        
        List<TableInfo> remaining = indegree.Where(kv => kv.Value > 0).Select(kv => kv.Key)
            .OrderBy(n => n.Name)
            .ToList();
        result.AddRange(remaining);

        return result;
    }
    
}