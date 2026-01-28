import csv


if __name__ == "__main__":
    header = []
    with open("LangResources.csv", "r", encoding="utf-8") as resource:
        reader = csv.reader(resource)
        header = next(reader, None)
        
    for i in range(1, len(header)):
        file_name = "LangResources." + header[i] + ".axaml"
        with open("LangResources.csv", "r", encoding="utf-8") as resource:
            reader = csv.reader(resource)
            next(reader, None)
            with open(file_name, "w", encoding="utf-8") as f:
                f.write('<ResourceDictionary\n    xmlns="https://github.com/avaloniaui"\n    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"\n    xmlns:system="clr-namespace:System;assembly=System.Runtime">\n\n')
                for row in reader:
                    if len(row) <= len(header):
                        continue
                    key = row[0]
                    value = row[i]
                    f.write('    <system:String x:Key="' + key + '">' + value + '</system:String>\n')
                f.write('\n</ResourceDictionary>\n')
            
