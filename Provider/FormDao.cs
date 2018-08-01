﻿using System;
using System.Collections.Generic;
using System.Data;
using SiteServer.Plugin;
using SS.Form.Model;

namespace SS.Form.Provider
{
    public class FormDao
    {
        public const string TableName = "ss_form";

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(FormInfo.Id),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.SiteId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.ChannelId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.ContentId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.Title),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.Description),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.IsTimeout),
                DataType = DataType.Boolean
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.TimeToStart),
                DataType = DataType.DateTime
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.TimeToEnd),
                DataType = DataType.DateTime
            },
            new TableColumn
            {
                AttributeName = nameof(FormInfo.Settings),
                DataType = DataType.Text
            }
        };

        private readonly string _connectionString;
        private readonly IDatabaseApi _helper;

        public FormDao(string connectionString, IDatabaseApi helper)
        {
            _connectionString = connectionString;
            _helper = helper;
        }

        public int Insert(FormInfo formInfo)
        {
            int formId;

            if (formInfo.ContentId == 0)
            {
                formInfo.Taxis = GetMaxTaxis(formInfo.SiteId) + 1;
            }

            string sqlString = $@"INSERT INTO {TableName}
           ({nameof(FormInfo.SiteId)}, 
            {nameof(FormInfo.ChannelId)}, 
            {nameof(FormInfo.ContentId)}, 
            {nameof(FormInfo.Title)}, 
            {nameof(FormInfo.Description)}, 
            {nameof(FormInfo.Taxis)}, 
            {nameof(FormInfo.IsTimeout)}, 
            {nameof(FormInfo.TimeToStart)}, 
            {nameof(FormInfo.TimeToEnd)}, 
            {nameof(FormInfo.Settings)})
     VALUES
           (@{nameof(FormInfo.SiteId)}, 
            @{nameof(FormInfo.ChannelId)}, 
            @{nameof(FormInfo.ContentId)}, 
            @{nameof(FormInfo.Title)}, 
            @{nameof(FormInfo.Description)}, 
            @{nameof(FormInfo.Taxis)}, 
            @{nameof(FormInfo.IsTimeout)}, 
            @{nameof(FormInfo.TimeToStart)}, 
            @{nameof(FormInfo.TimeToEnd)}, 
            @{nameof(FormInfo.Settings)})";

            var parameters = new List<IDataParameter>
            {
                _helper.GetParameter(nameof(formInfo.SiteId), formInfo.SiteId),
                _helper.GetParameter(nameof(formInfo.ChannelId), formInfo.ChannelId),
                _helper.GetParameter(nameof(formInfo.ContentId), formInfo.ContentId),
                _helper.GetParameter(nameof(formInfo.Title), formInfo.Title),
                _helper.GetParameter(nameof(formInfo.Description), formInfo.Description),
                _helper.GetParameter(nameof(formInfo.Taxis), formInfo.Taxis),
                _helper.GetParameter(nameof(formInfo.IsTimeout), formInfo.IsTimeout),
                _helper.GetParameter(nameof(formInfo.TimeToStart), formInfo.TimeToStart),
                _helper.GetParameter(nameof(formInfo.TimeToEnd), formInfo.TimeToEnd),
                _helper.GetParameter(nameof(formInfo.Settings), formInfo.Settings)
            };

            using (var conn = _helper.GetConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        formId = _helper.ExecuteNonQueryAndReturnId(TableName, nameof(FormInfo.Id), trans, sqlString, parameters.ToArray());

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }

            return formId;
        }

        public void Update(FormInfo formInfo)
        {
            string sqlString = $@"UPDATE {TableName} SET
                {nameof(FormInfo.SiteId)} = @{nameof(FormInfo.SiteId)}, 
                {nameof(FormInfo.ChannelId)} = @{nameof(FormInfo.ChannelId)}, 
                {nameof(FormInfo.ContentId)} = @{nameof(FormInfo.ContentId)}, 
                {nameof(FormInfo.Title)} = @{nameof(FormInfo.Title)}, 
                {nameof(FormInfo.Description)} = @{nameof(FormInfo.Description)},
                {nameof(FormInfo.Taxis)} = @{nameof(FormInfo.Taxis)}, 
                {nameof(FormInfo.IsTimeout)} = @{nameof(FormInfo.IsTimeout)}, 
                {nameof(FormInfo.TimeToStart)} = @{nameof(FormInfo.TimeToStart)}, 
                {nameof(FormInfo.TimeToEnd)} = @{nameof(FormInfo.TimeToEnd)}, 
                {nameof(FormInfo.Settings)} = @{nameof(FormInfo.Settings)}
            WHERE {nameof(FormInfo.Id)} = @{nameof(FormInfo.Id)}";

            var parameters = new List<IDataParameter>
            {
                _helper.GetParameter(nameof(formInfo.SiteId), formInfo.SiteId),
                _helper.GetParameter(nameof(formInfo.ChannelId), formInfo.ChannelId),
                _helper.GetParameter(nameof(formInfo.ContentId), formInfo.ContentId),
                _helper.GetParameter(nameof(formInfo.Title), formInfo.Title),
                _helper.GetParameter(nameof(formInfo.Description), formInfo.Description),
                _helper.GetParameter(nameof(formInfo.Taxis), formInfo.Taxis),
                _helper.GetParameter(nameof(formInfo.IsTimeout), formInfo.IsTimeout),
                _helper.GetParameter(nameof(formInfo.TimeToStart), formInfo.TimeToStart),
                _helper.GetParameter(nameof(formInfo.TimeToEnd), formInfo.TimeToEnd),
                _helper.GetParameter(nameof(formInfo.Settings), formInfo.Settings),
                _helper.GetParameter(nameof(formInfo.Id), formInfo.Id)
            };

            _helper.ExecuteNonQuery(_connectionString, sqlString, parameters.ToArray());
        }

        public void Delete(int formId)
        {
            if (formId <= 0) return;

            string sqlString = $"DELETE FROM {TableName} WHERE {nameof(FormInfo.Id)} = {formId}";
            _helper.ExecuteNonQuery(_connectionString, sqlString);

            Main.Instance.FieldDao.DeleteByFormId(formId);
            Main.Instance.LogDao.DeleteByFormId(formId);
        }

        public bool IsTitleExists(int siteId, string title)
        {
            var exists = false;

            string sqlString = $@"SELECT {nameof(FormInfo.Id)} FROM {TableName} WHERE 
    {nameof(FormInfo.SiteId)} = @{nameof(FormInfo.SiteId)} AND 
    {nameof(FormInfo.Title)} = @{nameof(FormInfo.Title)}";

            var parms = new[]
            {
                _helper.GetParameter(nameof(FormInfo.SiteId), siteId),
                _helper.GetParameter(nameof(FormInfo.Title), title)
            };

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString, parms))
            {
                if (rdr.Read() && !rdr.IsDBNull(0))
                {
                    exists = true;
                }
                rdr.Close();
            }

            return exists;
        }

        public int GetFormIdByContentId(int siteId, int channelId, int contentId)
        {
            if (siteId > 0 && channelId > 0 && contentId > 0)
            {
                return Main.Instance.Dao.GetIntResult($"SELECT {nameof(FormInfo.Id)} FROM {TableName} WHERE {nameof(FormInfo.SiteId)} = {siteId} AND {nameof(FormInfo.ChannelId)} = {channelId} AND {nameof(FormInfo.ContentId)} = {contentId}");
            }

            return 0;
        }

        public int GetFormIdByTitle(int siteId, string title)
        {
            if (siteId > 0 && !string.IsNullOrEmpty(title))
            {
                string sqlString = $"SELECT {nameof(FormInfo.Id)} FROM {TableName} WHERE {nameof(FormInfo.SiteId)} = @{nameof(FormInfo.SiteId)} AND {nameof(FormInfo.Title)} = @{nameof(FormInfo.Title)}";

                var parameters = new[]
                {
                    _helper.GetParameter(nameof(FormInfo.SiteId), siteId),
                    _helper.GetParameter(nameof(FormInfo.Title), title)
                };

                return Main.Instance.Dao.GetIntResult(sqlString, parameters);
            }

            return 0;
        }

        public List<FormInfo> GetFormInfoListNotInChannel(int siteId)
        {
            var list = new List<FormInfo>();

            string sqlString = $@"SELECT {nameof(FormInfo.Id)}, 
                {nameof(FormInfo.SiteId)}, 
                {nameof(FormInfo.ChannelId)}, 
                {nameof(FormInfo.ContentId)}, 
                {nameof(FormInfo.Title)}, 
                {nameof(FormInfo.Description)}, 
                {nameof(FormInfo.Taxis)}, 
                {nameof(FormInfo.IsTimeout)}, 
                {nameof(FormInfo.TimeToStart)}, 
                {nameof(FormInfo.TimeToEnd)}, 
                {nameof(FormInfo.Settings)}
            FROM {TableName} WHERE {nameof(FormInfo.SiteId)} = {siteId} AND {nameof(FormInfo.ChannelId)} = 0 AND {nameof(FormInfo.ContentId)} = 0 ORDER BY Taxis DESC";

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                while (rdr.Read())
                {
                    list.Add(GetFormInfo(rdr));
                }
                rdr.Close();
            }

            if (list.Count == 0)
            {
                list.Add(CreateDefaultForm(siteId));
            }

            return list;
        }

        private FormInfo CreateDefaultForm(int siteId)
        {
            var formInfo = new FormInfo
            {
                SiteId = siteId,
                ChannelId = 0,
                ContentId = 0,
                Title = "默认表单",
                Description = "系统创建的默认表单",
                IsTimeout = false,
                TimeToStart = DateTime.Now,
                TimeToEnd = DateTime.Now.AddMonths(3)
            };
            formInfo.Id = Insert(formInfo);

            Main.Instance.FieldDao.Insert(new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "姓名",
                PlaceHolder = "请输入您的姓名",
                FieldType = InputType.Text.Value,
                Settings = new FieldSettings
                {
                    IsRequired = true,
                    IsVisibleInList = true
                }.ToString()
            });
            Main.Instance.FieldDao.Insert(new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "手机",
                PlaceHolder = "请输入您的手机号码",
                FieldType = InputType.Text.Value,
                Settings = new FieldSettings
                {
                    IsRequired = true,
                    IsVisibleInList = true,
                    ValidateType = ValidateType.Mobile
                }.ToString()
            });
            Main.Instance.FieldDao.Insert(new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "年龄",
                PlaceHolder = "请输入您的年龄",
                FieldType = InputType.Text.Value,
                Settings = new FieldSettings
                {
                    IsRequired = true,
                    IsVisibleInList = true,
                    ValidateType = ValidateType.Integer
                }.ToString()
            });
            Main.Instance.FieldDao.Insert(new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "所在城市",
                PlaceHolder = "请输入您的所在城市",
                FieldType = InputType.Text.Value,
                Settings = new FieldSettings
                {
                    IsRequired = true,
                    IsVisibleInList = true
                }.ToString()
            });
            var sex = new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "性别",
                FieldType = InputType.Radio.Value,
                Settings = new FieldSettings
                {
                    IsRequired = true,
                    IsVisibleInList = true
                }.ToString()
            };

            sex.Id = Main.Instance.FieldDao.Insert(sex);
            sex.Items = new List<FieldItemInfo>
            {
                new FieldItemInfo
                {
                    FieldId = sex.Id,
                    FormId = formInfo.Id,
                    Value = "男"
                },
                new FieldItemInfo
                {
                    FieldId = sex.Id,
                    FormId = formInfo.Id,
                    Value = "女"
                }
            };
            Main.Instance.FieldItemDao.InsertItems(formInfo.Id, sex.Id, sex.Items);
            Main.Instance.FieldDao.Insert(new FieldInfo
            {
                FormId = formInfo.Id,
                Title = "留言",
                PlaceHolder = "请输入您的留言",
                FieldType = InputType.TextArea.Value
            });

            return formInfo;
        }

        public FormInfo GetFormInfoOrCreateIfNotExists(int siteId, int channelId, int contentId)
        {
            FormInfo formInfo = null;

            string sqlString = $@"SELECT {nameof(FormInfo.Id)}, 
            {nameof(FormInfo.SiteId)}, 
            {nameof(FormInfo.ChannelId)}, 
            {nameof(FormInfo.ContentId)}, 
            {nameof(FormInfo.Title)}, 
            {nameof(FormInfo.Description)}, 
            {nameof(FormInfo.Taxis)}, 
            {nameof(FormInfo.IsTimeout)}, 
            {nameof(FormInfo.TimeToStart)}, 
            {nameof(FormInfo.TimeToEnd)}, 
            {nameof(FormInfo.Settings)}
            FROM {TableName} WHERE {nameof(FormInfo.SiteId)} = {siteId} AND {nameof(FormInfo.ChannelId)} = {channelId} AND {nameof(FormInfo.ContentId)} = {contentId}";

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    formInfo = GetFormInfo(rdr);
                }
                rdr.Close();
            }

            if (formInfo == null)
            {
                formInfo = new FormInfo
                {
                    SiteId = siteId,
                    ChannelId = channelId,
                    ContentId = contentId,
                    IsTimeout = false,
                    TimeToStart = DateTime.Now,
                    TimeToEnd = DateTime.Now.AddMonths(3)
                };
                formInfo.Id = Insert(formInfo);
            }

            return formInfo;
        }

        public FormInfo GetFormInfo(int id)
        {
            FormInfo formInfo = null;

            string sqlString = $@"SELECT {nameof(FormInfo.Id)}, 
            {nameof(FormInfo.SiteId)}, 
            {nameof(FormInfo.ChannelId)}, 
            {nameof(FormInfo.ContentId)}, 
            {nameof(FormInfo.Title)}, 
            {nameof(FormInfo.Description)}, 
            {nameof(FormInfo.Taxis)}, 
            {nameof(FormInfo.IsTimeout)}, 
            {nameof(FormInfo.TimeToStart)}, 
            {nameof(FormInfo.TimeToEnd)}, 
            {nameof(FormInfo.Settings)}
            FROM {TableName} WHERE {nameof(FormInfo.Id)} = {id}";

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    formInfo = GetFormInfo(rdr);
                }
                rdr.Close();
            }

            return formInfo;
        }

        public bool UpdateTaxisToUp(int siteId, int formId)
        {
            var sqlString = _helper.GetPageSqlString(TableName, $"{nameof(FormInfo.Id)}, {nameof(FormInfo.Taxis)}", $"WHERE (({nameof(FormInfo.Taxis)} > (SELECT {nameof(FormInfo.Taxis)} FROM {TableName} WHERE {nameof(FormInfo.Id)} = {formId})) AND {nameof(FormInfo.SiteId)} ={siteId})", $"ORDER BY {nameof(FormInfo.Taxis)}", 0, 1);

            var higherId = 0;
            var higherTaxis = 0;

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    higherId = rdr.GetInt32(0);
                    higherTaxis = rdr.GetInt32(1);
                }
                rdr.Close();
            }

            var selectedTaxis = GetTaxis(formId);

            if (higherId != 0)
            {
                SetTaxis(formId, higherTaxis);
                SetTaxis(higherId, selectedTaxis);
                return true;
            }
            return false;
        }

        public bool UpdateTaxisToDown(int siteId, int formId)
        {
            var sqlString = _helper.GetPageSqlString(TableName, $"{nameof(FormInfo.Id)}, {nameof(FormInfo.Taxis)}", $"WHERE (({nameof(FormInfo.Taxis)} < (SELECT {nameof(FormInfo.Taxis)} FROM {TableName} WHERE ({nameof(FormInfo.Id)} = {formId}))) AND {nameof(FormInfo.SiteId)} = {siteId})", $"ORDER BY {nameof(FormInfo.Taxis)} DESC", 0, 1);

            var lowerId = 0;
            var lowerTaxis = 0;

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    lowerId = rdr.GetInt32(0);
                    lowerTaxis = rdr.GetInt32(1);
                }
                rdr.Close();
            }

            var selectedTaxis = GetTaxis(formId);

            if (lowerId != 0)
            {
                SetTaxis(formId, lowerTaxis);
                SetTaxis(lowerId, selectedTaxis);
                return true;
            }
            return false;
        }

        private int GetMaxTaxis(int siteId)
        {
            var sqlString =
                $"SELECT MAX({nameof(FormInfo.Taxis)}) FROM {TableName} WHERE {nameof(FormInfo.SiteId)} = {siteId}";
            return Main.Instance.Dao.GetIntResult(sqlString);
        }

        private int GetTaxis(int formId)
        {
            var sqlString = $"SELECT {nameof(FormInfo.Taxis)} FROM {TableName} WHERE ({nameof(FormInfo.Id)} = {formId})";
            return Main.Instance.Dao.GetIntResult(sqlString);
        }

        private void SetTaxis(int formId, int taxis)
        {
            string sqlString = $"UPDATE {TableName} SET {nameof(FormInfo.Taxis)} = {taxis} WHERE {nameof(FormInfo.Id)} = {formId}";
            _helper.ExecuteNonQuery(_connectionString, sqlString);
        }

        private static FormInfo GetFormInfo(IDataRecord rdr)
        {
            if (rdr == null) return null;
            
            var formInfo = new FormInfo();

            var i = 0;
            formInfo.Id = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            formInfo.SiteId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            formInfo.ChannelId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            formInfo.ContentId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            formInfo.Title = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
            i++;
            formInfo.Description = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
            i++;
            formInfo.Taxis = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            formInfo.IsTimeout = !rdr.IsDBNull(i) && rdr.GetBoolean(i);
            i++;
            formInfo.TimeToStart = rdr.IsDBNull(i) ? DateTime.Now : rdr.GetDateTime(i);
            i++;
            formInfo.TimeToEnd = rdr.IsDBNull(i) ? DateTime.Now : rdr.GetDateTime(i);
            i++;
            formInfo.Settings = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);

            return formInfo;
        }

    }
}
