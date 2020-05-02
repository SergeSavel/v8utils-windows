// Copyright 2020 Sergey Savelev
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using SSavel.V8Utils.Platform;

namespace SSavel.V8Utils.Windows.Platform.Entities
{
    internal partial class InfobaseDescription
    {
        internal readonly ScriptVariant ScriptVariant;

        internal readonly List<Table> Structure;

        private InfobaseDescription(dynamic connection, IInfobase infobase)
        {
            ScriptVariant = GetScriptVariant(connection);
            Structure = GetStructure(connection, ScriptVariant);
        }

        internal class Table
        {
            public List<TableField> Fields = new List<TableField>();
            public List<TableIndex> Indexes = new List<TableIndex>();
            public string StorageTableName, TableName, Metadata, Purpose;

            public class TableField
            {
                public string StorageFieldName, FieldName;
            }

            public class TableIndex
            {
                public List<TableIndexField> Fields = new List<TableIndexField>();
                public string StorageIndexName;

                public class TableIndexField
                {
                    public string StorageFieldName, FieldName;
                }
            }
        }
    }

    internal partial class InfobaseDescription // Static members
    {
        //static internal InfobaseDescription Get(ComConnector connector, Infobase infobase)
        //{
        //	dynamic connection = connector.Connect(infobase);

        //	if (connection == null)
        //		return null;

        //	return new InfobaseDescription(connection, infobase);
        //}

        private static ScriptVariant GetScriptVariant(dynamic connection)
        {
            var metadata = connection.Metadata;
            var objectProperties = metadata.ObjectProperties;
            var enumScriptVariant = objectProperties.ScriptVariant;
            var scriptVariant = metadata.ScriptVariant;
            var scriptVariantEnglish = enumScriptVariant.English;
            //var scriptVariantRussian = enumScriptVariant.Russian;

            var sv = scriptVariant == scriptVariantEnglish ? ScriptVariant.English : ScriptVariant.Russian;

            //Marshal.ReleaseComObject(scriptVariantRussian);
            Marshal.ReleaseComObject(scriptVariantEnglish);
            Marshal.ReleaseComObject(scriptVariant);
            Marshal.ReleaseComObject(enumScriptVariant);
            Marshal.ReleaseComObject(objectProperties);
            Marshal.ReleaseComObject(metadata);

            return sv;
        }

        private static List<Table> GetStructure(dynamic connection, ScriptVariant scriptVariant)
        {
            var result = new List<Table>();

            dynamic dbStorageStructureInfo = null;
            try
            {
                dbStorageStructureInfo = connection.GetDBStorageStructureInfo(null, true);

                for (var i = 0; i < dbStorageStructureInfo.Count(); i++)
                {
                    var table = new Table();
                    result.Add(table);

                    var record = dbStorageStructureInfo.Get(i);
                    dynamic fields = null;
                    dynamic indexes = null;

                    if (scriptVariant == ScriptVariant.English)
                    {
                        table.StorageTableName = record.StorageTableName;
                        table.TableName = record.TableName;
                        table.Metadata = record.Metadata;
                        table.Purpose = record.Purpose;

                        fields = record.Fields;
                        indexes = record.Indexes;
                    }
                    else
                    {
                        table.StorageTableName = record.ИмяТаблицыХранения;
                        table.TableName = record.ИмяТаблицы;
                        table.Metadata = record.Метаданные;
                        table.Purpose = record.Назначение;

                        fields = record.Поля;
                        indexes = record.Индексы;
                    }

                    for (var j = 0; j < fields.Count(); j++)
                    {
                        var fieldRecord = fields.Get(j);

                        var field = new Table.TableField();
                        table.Fields.Add(field);

                        if (scriptVariant == ScriptVariant.English)
                        {
                            field.StorageFieldName = fieldRecord.StorageFieldName;
                            field.FieldName = fieldRecord.FieldName;
                        }
                        else
                        {
                            field.StorageFieldName = fieldRecord.ИмяПоляХранения;
                            field.FieldName = fieldRecord.ИмяПоля;
                        }

                        Marshal.ReleaseComObject(fieldRecord);
                    }

                    for (var j = 0; j < indexes.Count(); j++)
                    {
                        var indexRecord = indexes.Get(j);
                        dynamic indexFields = null;

                        var index = new Table.TableIndex();
                        table.Indexes.Add(index);

                        if (scriptVariant == ScriptVariant.English)
                        {
                            index.StorageIndexName = indexRecord.StorageIndexName;

                            indexFields = indexRecord.Fields;
                        }
                        else
                        {
                            index.StorageIndexName = indexRecord.ИмяИндексаХранения;

                            indexFields = indexRecord.Поля;
                        }

                        for (var k = 0; k < indexFields.Count(); k++)
                        {
                            var indexFieldRecord = indexFields.Get(k);

                            var indexField = new Table.TableIndex.TableIndexField();
                            index.Fields.Add(indexField);

                            if (scriptVariant == ScriptVariant.English)
                            {
                                indexField.StorageFieldName = indexFieldRecord.StorageFieldName;
                                indexField.FieldName = indexFieldRecord.FieldName;
                            }
                            else
                            {
                                indexField.StorageFieldName = indexFieldRecord.ИмяПоляХранения;
                                indexField.FieldName = indexFieldRecord.ИмяПоля;
                            }

                            Marshal.ReleaseComObject(indexFieldRecord);
                        }

                        Marshal.ReleaseComObject(indexFields);
                        Marshal.ReleaseComObject(indexRecord);
                    }

                    Marshal.ReleaseComObject(fields);
                    Marshal.ReleaseComObject(indexes);
                    Marshal.ReleaseComObject(record);
                }
            }
            finally
            {
                if (dbStorageStructureInfo != null) Marshal.ReleaseComObject(dbStorageStructureInfo);
            }

            return result;
        }
    }
}