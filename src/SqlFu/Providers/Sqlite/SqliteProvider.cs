﻿using System;
using System.Data.Common;
using CavemanTools.Model;
using SqlFu.Builders;

namespace SqlFu.Providers.Sqlite
{
    public class SqliteProvider:DbProvider
    {
        public const string Id = "Sqlite";
        public readonly SqliteType DbTypes = new SqliteType();


        public SqliteProvider(Func<DbConnection> factory) : base(factory, Id)
        {
        }

        protected override EscapeIdentifierChars GetEscapeIdentifierChars()
        =>new EscapeIdentifierChars('[',']');

        public override string ParamPrefix { get; } = "@";
        public override string GetColumnType(Type type)
        {
            if (type.IsEnumType()) type = typeof(int);
            return DbTypes[type];
        }

        public override string GetIdentityKeyword()
            => "primary key autoincrement";

        public override bool IsDbBusy(DbException ex)
        {
            return ex.Message.Contains("locked");
        }

        public override bool IsUniqueViolation(DbException ex, string keyName = "")
        {
            if (!ex.Message.Contains("UNIQUE constraint failed:")) return false;
            if (keyName.IsNullOrEmpty()) return false;
            return ex.Message.Contains(keyName);
        }

        public override bool ObjectExists(DbException ex, string name = null)
        {
            if (!ex.Message.Contains("already exists")) return false;
            if (name.IsNullOrEmpty()) return false;
            return ex.Message.Contains(name);
        }

        public override string AddReturnInsertValue(string sqlValues, string identityColumn)
            => $"{sqlValues};SELECT last_insert_rowid()";

        public override string FormatQueryPagination(string sql, Pagination page, ParametersManager pm) 
            => $"{sql} limit {page.Skip},{page.PageSize}";

        protected override IDatabaseTools InitTools()
       => new SqliteDbTools(this);

        protected override IDbProviderExpressions InitExpressionHelper()
        => new DbProviderExpressions();
    }
}