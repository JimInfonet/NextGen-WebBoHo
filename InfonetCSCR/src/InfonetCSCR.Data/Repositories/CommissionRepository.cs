namespace InfonetCSCR.Data.Repositories;

public class CommissionRepository : BaseRepository, ICommissionRepository
{
	public CommissionRepository(IDbTransaction transaction) : base(transaction) { }

	public async Task<IEnumerable<Commission>> GetCommissionListAsync(string employeeID)
	{
		const string selectCommissionListQuery = @"
				SELECT
					EC.[EmployeeID],
					U.[U_NAME] AS [EmployeeName],
					EC.[Dept] AS [Department],
					EC.[SubDept],
					EC.[SubDetail],
					LTRIM(RTRIM(EC.[StockCode])) AS [StockCode],
					EC.[Name] AS [GroupName],
					EC.[Commission] AS [CommissionPercentage],
					EC.[CommissionByDol] AS [CommissionAmount],
					EC.[Apply]
				FROM [CSCMaster].[dbo].[EmpCommission] AS EC
				INNER JOIN [CSCAdmin].[dbo].[USER] AS U ON EC.[EmployeeID] = U.[U_CODE]
				UNION
				SELECT
					[EmployeeID],
					'' AS [EmployeeName],
					[Dept] AS [Department],
					[SubDept],
					[SubDetail],
					LTRIM(RTRIM([StockCode])) AS [StockCode],
					[Name] AS [GroupName],
					[Commission] AS [CommissionPercentage],
					[CommissionByDol] AS [CommissionAmount],
					[Apply]
				FROM [CSCMaster].[dbo].[EmpCommission]
				WHERE [EmployeeID] = @EmployeeID
				ORDER BY [EmployeeID], [StockCode], [Dept], [SubDept], [SubDetail]";

		return await Connection.QueryAsync<Commission>(selectCommissionListQuery, new { EmployeeID = employeeID }, Transaction);
	}

	public async Task<bool> CommissionExistsAsync(Commission commission)
	{
		const string commmissionExistsQuery = @"
        SELECT COUNT(*)
        FROM [CSCMaster].[dbo].[EmpCommission]
        WHERE
            [EmployeeID] = @EmployeeID AND
            [Dept] = @Department AND
            [SubDept] = @SubDept AND
            [SubDetail] = @SubDetail AND
            LTRIM(RTRIM([StockCode])) = @StockCode";

		var count = await Connection.ExecuteScalarAsync<int>(commmissionExistsQuery, commission, Transaction);
		return count > 0;
	}

	public async Task AddCommissionAsync(Commission commission)
	{
		var addCommissionQuery = new StringBuilder();

		addCommissionQuery.AppendLine(@"
				INSERT INTO [CSCMaster].[dbo].[EMPCOMMISSION] (
					[EmployeeID],
					[Dept],
					[SubDept],
					[SubDetail],
					[StockCode],
					[Commission],
					[CommissionByDol],
					[Apply],
					[Name])
				SELECT
					@EmployeeID,
					@Department,
					@SubDept,
					@SubDetail,
					@StockCode,
					@CommissionPercentage,
					@CommissionAmount,
					@Apply,");

		if (commission.StockCode != commission.AllOption)
		{
			addCommissionQuery.Append(@"
						ST.[DESCRIPT] 
					FROM [CSCMaster].[dbo].[STOCKMST] AS ST 
					WHERE LTRIM(RTRIM(ST.[STOCK_CODE])) = @StockCode");
		}
		else
		{
			addCommissionQuery.Append("@GroupName");
		}

		await Connection.ExecuteAsync(addCommissionQuery.ToString(), commission, Transaction);
	}

	public async Task AddEmpCommissionDataAsync(Commission commission)
	{
		var addEmpCommissionDataQuery = new StringBuilder();
		bool andCondition = false;

		addEmpCommissionDataQuery.AppendLine(@"
				INSERT INTO [CSCTrans].[dbo].[EmpCommissionData] (
					[EmployeeID],
					[Dept],
					[SubDept],
					[SubDetail],
					[StockCode],
					[ByStockCode],
					[Commission],
					[CommissionByDol],
					[Apply])
				SELECT
					@EmployeeID,
					@Department,
					@SubDept,
					@SubDetail,
					LTRIM(RTRIM(ST.[Stock_Code])),
					@ByStockCode,
					@CommissionPercentage,
					@CommissionAmount,
					@Apply
				FROM [CSCMaster].[dbo].[STOCKMST] AS ST");

		if (commission.StockCode != commission.AllOption)
		{
			commission.ByStockCode = true;
			addEmpCommissionDataQuery.Append("WHERE LTRIM(RTRIM(ST.[STOCK_CODE])) = @StockCode");
		}
		else
		{
			if (commission.Department != commission.AllOption)
			{
				addEmpCommissionDataQuery.AppendLine("WHERE ST.[DEPT] = @Department");
				andCondition = true;
			}

			if (commission.SubDept != commission.AllOption)
			{
				addEmpCommissionDataQuery.Append(andCondition ? "AND" : "WHERE");
				addEmpCommissionDataQuery.AppendLine(" ST.[SUB_DEPT] = @SubDept");
				andCondition = true;
			}

			if (commission.SubDetail != commission.AllOption)
			{
				addEmpCommissionDataQuery.Append(andCondition ? "AND" : "WHERE");
				addEmpCommissionDataQuery.AppendLine(" ST.[SUB_DETAIL] = @SubDetail");
			}
		}

		await Connection.ExecuteAsync(addEmpCommissionDataQuery.ToString(), commission, Transaction);
	}

	public async Task EditCommissionAsync(Commission commission)
	{
		const string editCommissionQuery = @"
				UPDATE [CSCMaster].[dbo].[EmpCommission]
				SET	
					[Commission] = @CommissionPercentage,
					[CommissionByDol] = @CommissionAmount,
					[Apply] = @Apply 
				WHERE
					[EmployeeID] = @EmployeeID AND
					LTRIM(RTRIM([StockCode])) = @StockCode AND
					[Dept] = @Department AND
					[SubDept] = @SubDept AND
					[SubDetail] = @SubDetail";

		await Connection.ExecuteAsync(editCommissionQuery, commission, Transaction);
	}

	public async Task EditEmpCommissionDataAsync(Commission commission)
	{
		var editEmpCommissionDataQuery = new StringBuilder();

		editEmpCommissionDataQuery.AppendLine(@"
				UPDATE [CSCTrans].[dbo].[EmpCommissionData]
				SET
					[Commission] = @CommissionPercentage,
					[CommissionByDol] = @CommissionAmount,
					[Apply] = @Apply
				WHERE
					[EmployeeID] = @EmployeeID AND
					[Dept] = @Department AND
					[SubDept] = @SubDept AND
					[SubDetail] = @SubDetail AND
					[ByStockCode] = @ByStockCode");

		if (commission.StockCode != commission.AllOption)
		{
			editEmpCommissionDataQuery.Append("AND LTRIM(RTRIM([StockCode])) = @StockCode");
			commission.ByStockCode = true;
		}

		await Connection.ExecuteAsync(editEmpCommissionDataQuery.ToString(), commission, Transaction);
	}

	public async Task DeleteCommissionAsync(Commission commission)
	{
		const string deleteCommissionQuery = @"
				DELETE FROM [CSCMaster].[dbo].[EmpCommission]
				WHERE 
					[EmployeeID] = @EmployeeID AND 
					LTRIM(RTRIM([StockCode])) = @StockCode AND
					[Dept] = @Department AND
					[SubDept] = @SubDept AND
					[SubDetail] = @SubDetail AND
					[Commission] = @CommissionPercentage AND
					[CommissionByDol] = @CommissionAmount AND
					[Apply] = @Apply";

		await Connection.ExecuteAsync(deleteCommissionQuery, commission, Transaction);
	}

	public async Task DeleteEmpCommissionDataAsync(Commission commission)
	{
		var deleteEmpCommissionDataQuery = new StringBuilder();

		deleteEmpCommissionDataQuery.AppendLine(@"
				DELETE FROM [CSCTrans].[dbo].[EmpCommissionData]
				WHERE
						[EmployeeID] = @EmployeeID AND
						[Dept] = @Department AND
						[SubDept] = @SubDept AND
						[SubDetail] = @SubDetail AND
						[ByStockCode] = @ByStockCode");

		if (commission.StockCode != commission.AllOption)
		{
			deleteEmpCommissionDataQuery.Append("AND LTRIM(RTRIM([StockCode])) = @StockCode");
			commission.ByStockCode = true;
		}

		await Connection.ExecuteAsync(deleteEmpCommissionDataQuery.ToString(), commission, Transaction);
	}
}
