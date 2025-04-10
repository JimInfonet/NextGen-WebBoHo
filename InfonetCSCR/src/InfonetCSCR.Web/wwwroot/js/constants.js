/*:::::::::::::::: General Start ::::::::::::::::*/
const GENERAL_STRING_TABLE = {
  URL: {
    ADD_ITEMS_TO_THE_PO: "AddItemsToThePO",
    GET_USER_DETAILS: "Auth/GetUserDetails",
    PURCHASE_DETAIL: "PurchaseDetail",
    SAVE_FILE: "File/SaveFile",
    SAVE_LOG_WITH_BACKUP: "File/SaveLogWithBackup",
    BASE64_IMAGE_PREFIX: "data:image/jpeg;base64,"
  },
  PARAMS: {
    PURCHASE_NO: "purchaseNo",
    PURCHASE_ORDER_NO: "purchaseOrderNumber",
    USER: "user",
    USER_CODE: "userCode",
    VENDOR_ID: "vendorId",
  },
  FILE_TYPE: {
    PY_FILE: "TimesheetPayPeriodPYFile",
    FLEET_CARD_SALE_REPORT: "FleetCardSaleReport",
    CUST_SALES_DETAIL_REPORT: "CustomerSalesDetailReport",
  }
}

/*:::::::::::::::: General End ::::::::::::::::*/

const tenderClass = {
  "CRCARD": "CRCARD",
  "DBCARD": "DBCARD",
  "FLEET": "FLEET",
  "GIFTCARD": "GIFTCARD",
  "LOYALTY": "LOYALTY",
  "THIRDPARTY": "THIRDPARTY",
}

/*:::::::::::::::: Pricebook Transfer Start ::::::::::::::::*/

const PRICEBOOK_TRANSFER_STRING_TABLE = {
  URL: {
    PRICEBOOK_TRANSFER: "/PricebookTransfer",
    REGION: "Region",
    STOCK_PRICE: "StockPrice",
    STORE: "Store",
    GET: {
      DEPARTMENTS: "/PricebookTransfer/GetDepartments",
      NEW_BATCH_ID: "/PricebookTransfer/GetNewBatchId",
      POLICIES: "PricebookTransfer/GetPolicies",
      REGION: "/Region/GetRegion",
      SUB_DEPARTMENTS: "/PricebookTransfer/GetSubDepartments",
      SUB_DETAILS: "/PricebookTransfer/GetSubDetails",
      PRICE_GROUP: "/StockPrice/GetPriceGroup",
      COST_CHANGE_TRANSFERS: "PricebookTransfer/GetCostChangeTransfers",
      STORE_IDS_BY_VENDOR: "/Store/GetStoreIdsByVendor",
      STORE_IDS_BY_VENDOR_IDS: "/Store/GetStoreIdsByVendorIds",
      STORES_WITH_REGION_AND_PRICE_GROUP: "/Store/GetStoresWithRegionAndPriceGroup",
      STOCK_ITEMS_BY_DEPTS_SUB_DEPTS_AND_SUBDETAILS: "/StockItems/GetStockItemsByDeptSubDeptAndSubDetail",
      STOCK_COST_INFO: "/StockItems/GetStockCostInfo",
      PACKING_INFO: "/StockItems/GetPackingInfo",
      STOCK_ITEMS_BY_VENDOR: "/StockItems/GetStockItemsByVendor",
      STOCK_PRICE_INFO: "/StockPrice/GetStockPriceInfo",
      PENDING_TRANSFER_STORES: "/PricebookTransfer/GetPendingTransferStores",
      VENDORS: "/PricebookTransfer/GetVendors",
      PRICE_CHANGE_TRANSFERS: "/PricebookTransfer/GetPriceChangeTransfers",
      LABEL_PRINTS: "/PricebookTransfer/GetLabelPrints",
      COMMODITY_TRANSFERS: "/PricebookTransfer/getCommodityTransfers",
      PROMOTION_TRANSFERS: "/PricebookTransfer/GetPromotionTransfers",
      HOT_BUTTONS_TRANSFER: "/PricebookTransfer/getHotButtonsTransfers",
      PROMOTIONS: "/PricebookTransfer/GetPromotions",
      HOT_BUTTONS: "/PricebookTransfer/GetHotButtons",
    },
    POST: {
      PERFORM_TRANSFER: "/PricebookTransfer/PerformTransfer"
    },
    INSERT: {
      PRICE_CHANGE_TRANSFER: "/PricebookTransfer/InsertPriceChangeTransfer",
      PRICE_CHANGE_TRANSFER_BATCH: "/PricebookTransfer/InsertPriceChangeTransferBatch",
      PRICE_DEFINATION: "/PricebookTransfer/InsertPriceDefination",
      PRICE_DEFINATION_BATCH: "/PricebookTransfer/InsertPriceDefinationBatch",
      PENDING_TRANSFERS: "/PricebookTransfer/PendingTransfers",
      PENDING_COMMODITY_TRANSFERS: "/PricebookTransfer/PendingCommodityTransfers",
      PENDING_PROMOTION_TRANSFERS: "/PricebookTransfer/PendingPromotionTransfers",
      PENDING_HOT_BUTTONS_TRANSFERS: "/PricebookTransfer/PendingHotButtonsTransfers",
    },
    DELETE: {
      COMMODITY_TRANSFER: "/PricebookTransfer/deleteCommodityTransfer",
      FROM_TRANSFER_LIST: "/PricebookTransfer/DeleteFromTransferList",
    },
  },
  DATA_TRANSFER_TYPE: {
    PRICE_CHANGE: "price-change",
    COST_CHANGE: "cost-change",
    COMMODITY: "commodity",
    STOCKS: "stocks",
    HOT_BUTTONS: "hotButtons",
    PROMOTIONS: "promotions",
    LABEL_PRINTING: "label-printing",
  },
  DELETE: {
    ALL: "ALL",
    STORE: "STORE",
    ITEM: "ITEM",
  },
  PRICE_OPTION: {
    CURRENT: "current-price",
    FUTURE: "future-price",
  },
  PRICE_SETUP: {
    CURRENCY_DOLLAR: "currency-dollar",
    CURRENCY_PERCENTAGE: "currency-percentage",
    INCREASE: "increase",
    DECREASE: "decrease",
    CHANGE_BY_MARGIN: "change-by-margin",
    CHANGE_BY_PRICE: "change-by-price",
  },
  PRICE_TYPE: {
    R_REGULAR_PRICE: "r-regular-price",
    S_SALE_PRICING: "s-sale-pricing",
    F_FIRST_UNIT_PRICING: "f-first-unit-pricing",
    I_INCREMENTAL_PRICING: "i-incremental-pricing",
    X_X_FOR_PRICING: "x-x-for-pricing",
  },
  SHOW_BY_OPTION: {
    BY_STORE: "by-store",
    BY_REGION: "by-region",
    BY_PRICE_GROUP: "by-price-group",
    BY_VENDOR: "by-vendor",
  },
  TABLE: {
    COLUMN_NAME: {
      BATCH: "batch",
      BATCH_ID: "batchId",
      DEPT_ID: "deptId",
      DEPT_NAME: "deptName",
      DESCRIPTION: "description",
      PRODUCT: "product",
      PRODUCT_ID: "productId",
      STOCK_CODE: "stockCode",
      SUB_DEPT_ID: "subDeptId",
      SUB_DEPT_NAME: "subDeptName",
      SUB_DETAIL_ID: "subDetailId",
      SUB_DETAIL_NAME: "subDetailName",
      PRICE_TYPE: "priceType",
      PROMO_ID: "promoId",
      START_DATE: "startDate",
      END_DATE: "endDate",
      TRANS_TYPE: "transType",
      TR_TYPE: "trType",
      TYPE: "type",
    },
    SEARCH_COLUMN_NAME: {
      ID: "Product ID", // DO NOT CHANGE
      NAME: "Product Name", // DO NOT CHANGE
    },
  },
  PRICE_CHANGE: "price-change",
  PARAMS: {
    STOCK_CODE: "stockCode",
    VENDOR_ID: "vendorId",
    PRICE_NUMBER: "priceNumber",
    ORDER_NO: "orderNo",
    DATA_TRANSFER_TYPE: "dataTransferType",
    BATCH_ID: "batchId",
    ITEM_ID: "itemId",
    DESCRIPTION: "description",
    STORE_ID: "storeId",
    DELETE_ALL: "deleteAll"
  },
  DEFAULT: {
    STORE_ID: "ALL",
    VENDOR_ID: "ALL",
  },
  DOLLAR_SIGN: "$",
  PERCENTAGE_SIGN: "%",
}

/*:::::::::::::::: Pricebook Transfer End ::::::::::::::::*/


/*:::::::::::::::: Check In And Check Out Start ::::::::::::::::*/

const CHEKC_IN_CHECK_OUT_STRING_TABLE = {
  URL: {
    ADD_COMMENT: "/CheckInCheckOut/AddComment",
    AUTHENTICATE_USER: "/CheckInCheckOut/AuthenticateUser",
    DETERMINE_IN_OR_OUT: "/CheckInCheckOut/DetermineInOrOut",
    GET_CURRENT_USERS: "/CheckInCheckOut/GetCurrentUsers",
    GET_POLICIES: "/CheckInCheckOut/GetPolicies",
    GET_USER_ID: "/Auth/GetUserId",
    GET_USERNAME_BY_CODE: "/Auth/GetUsernameByCode",
    IS_VALID_USER: "/CheckInCheckOut/IsValidUser",
    PERFORM_CHECK_OR_OUT: "/CheckInCheckOut/PerformCheckInOrOut",
  },
  PARAMS: {
    CHECK_OUT_PREVIOUS_DAY: "checkOutPreviousDay",
    CODE: "code",
    COMMENT_FIELD_VALUE: "commentFieldValue",
    IS_CHECK_IN: "inCheckIn",
    NUM_REC: "numRec",
    POLICIES: "policies",
    POLICY_HOUR24STORE: "policyHOUR24STORE",
    USER_CODE: "userCode",
    TEXT: "text",
  }
}

/*:::::::::::::::: Check In And Check Out End ::::::::::::::::*/


/*:::::::::::::::: Timesheet Reports Start ::::::::::::::::*/

const TIMESHEET_REPORTS_STRING_TABLE = {
  URL: {
    CHECK_IN_CHECK_OUT: "/CheckInCheckOut",
    SAVE_FILE: "File/SaveFile",
    GENERATE_PY_FILE: "/TimesheetReports/GeneratePYFile",
    GET_OUTSTANDING_SHORTAGES: "/TimesheetReports/GetOutstandingShortages",
    GET_POLICIES: "/CheckInCheckOut/GetPolicies",
    GET_REPORTS: "/TimesheetReports/GetReports",
    GET_TIMESHEET_DETAILS: "/TimesheetReports/GetTimesheetDetails",
    GET_TIMESHEET_HEADER_DETAILS: "/TimesheetReports/GetTimesheetHeaderDetails",
    MISSING_CHECK_OUTS_EXIST: "/TimesheetReports/MissingCheckOutsExist",
    MISSING_COMMENT_EXIST: "/TimesheetReports/MissingCommentExist",
    POST_PAY_PERIOD: "/TimesheetReports/PostPayPeriod",
    UPDATE_SHORTAGES: "/TimesheetReports/UpdateShortages",
    UPDATE_MISSING_REPORT_ID_IN_SHORTAGES: "/TimesheetReports/UpdateMissingReportIdInShortages",
    UPDATE_SHIFT_FORM: "/TimesheetReports/UpdateShiftForm",
    UDPATE_UNIFORM_PURCHASES: "/TimesheetReports/UpdateUniformPurchases",
  },
  PARAMS: {
    IS_HOURLY: "isHourly",
    POLICIES: "policies",
    PERIOD_STARTING: "periodStarting",
    PERIOD_ENDING: "periodEnding",
    REPORT_ID: "reportId",
    SHIFT_DATE: "shiftDate",
    UNIFORM_PURCHASES: "uniformPurchases",
    USER_CODE: "userCode",
    USER_ID: "userId",
  },
  TABLE: {
    TIMESHEET_DETAILS: {
      COLUMN_NAME: {
        DAY_OF_WEEK: "dayOfWeek",
        SHIFT_DATE: "shiftDate",
        STAT: "stat",
        TIME_IN: "timeIn",
        TIME_OUT: "timeOut",
        HOURS: "hours",
        COMMISSION: "commission",
        SHORTAGES: "shortages",
        TRAINING: "training",
        COMMENT: "comment",
      },
    },
    EMPLOYEE_LIST: {
      COLUMN_NAME: {
        USER_CODE: "userCode",
        PERIOD_STARTING: "periodStarting",
        PERIOD_ENDING: "periodEnding",
        STATUS: "status",
      },
    },
    INSIDE_DETAILS: {
      COLUMN_NAME: {
        SHIFT_DATE: "shiftDate",
        SHORTAGE: "shortage",
      },
    }
  },
};

const STOCK_TAKES_AND_RECEIVING_STRING_TABLE = {
  TABLE: {
    SEARCH_COLUMN_NAME: {
      CODE: "Stock Code", // DO NOT CHANGE
      DESCRIPTION: "Description", // DO NOT CHANGE
    },
  }
};
/*:::::::::::::::: Timesheet Reports End ::::::::::::::::*/


/*:::::::::::::::: Add items to the PO Start ::::::::::::::::*/

const ADD_ITEMS_TO_THE_PO_STRING_TABLE = {
  URL: {
    ADD_VENDOR_PARTS: "AddItemsToThePO/AddVendorParts",
    GET_POLICIES: "PurchasingAndReceiving/GetPurchasingAndReceivingPolicies",
    GET_VENDOR_PARTS: "AddItemsToThePO/GetVendorParts",
    GET_EXISTING_PART_NOS: "AddItemsToThePO/GetExistingPartNos",
  },
  PARAMS: {
    PART_NOS: "partNos",
    POLICIES: "policies",
    PURCHASE_NO: "purchaseNo",
    SELECTED_CRITEIRIA: "selectedCriteria",
    VENDOR_ID: "vendorId",
    VENDOR_PARTS: "vendorParts",
  },
  TABLE: {
    COLUMN_NAME: {
      PART_NO: "partNo",
      STOCK_CODE: "stockCode",
      VENDOR_ID: "vendorId",
      DESCRIPTION: "description",
    }
  },
  ROW_TEXT: {
    VENDOR_PARTS_FOR_THE_VENDOR_X: "VendorPartsForTheVendorX",
  },
  MODAL_DATA_TABLE_ROW_DIMENSION: {
    HEIGHT: 39.6667,
  }
}

/*:::::::::::::::: Add items to the PO End ::::::::::::::::*/


const MODEL_VALIDATION_DATA_LIMIT = {
  SMALL_INTEGER_LIMIT: 32767,
  SMALL_INTEGER_LOWER_LIMIT: -32768,
  SMALL_DOUBLE_LIMIT: 214748.3647,
  SMALL_DOUBLE_LOWER_LIMIT: -214748.3648,
  REAL_LOWER_LIMIT: -3.402823466E+38,
  REAL_UPPER_LIMIT: 3.402823466E+38,
  SMALL_MONEY_UPPER_LIMIT: 214478.3647,
}

const CONSTANT_TEXTS = {
  DEPARTMENT: "Department",
  SUB_DEPARTMENT: "SubDepartment",
  DETAIL: "Detail",
  ORDER_QUANTITY: "OrderQuantity",
  PACKING_QUANTITY: "PackingQuantity",
  RECEIVED_QUANTITY: "ReceivedQuantity",
  PO_COST: "POCost",
  APPLY_GST: "ApplyGST",
  BUYER: "Buyer",
  SHIP_VIA: "ShipVia",
  FOB: "FOB",
  FREIGHT: "Freight",
  CUSTOMERACCOUNT: "CustomerAccount",
  PO_DATE: "PODate",
  TO_BE_DELIVERED: "ToBeDelivered",
  DATE_RECEIVED: "DateReceived",
  INVOICE_NUMBER: "InvoiceNumber",
  TERMS: "Terms",
  NOTES: "Notes",
  SUB_TOTAL: "SubTotal",
  SURCHARGE_DISCOUNT: "SurchargeDiscount",
  TAX_TOTAL: "TaxTotal",
  TOTAL: "Total",
  UPDATE: "update",
  ADD: "add",
  ROW_DOUBLE_CLICK: "RowDoubleClick",
  VIEW: "view",
  CREATE: "create",
  COPY: "copy",
  POSITIVE_CARDS: "PositiveCards",
  NEGATIVE_CARDS: "NegativeCards",
  MESSAGES: "Messages", 
  EDIT: "edit"
}

// Customer Start

const CUSTOMER_STRING_TABLE = {
  URL: {
    GET: {
      POLICIES: "/Customer/GetPolicies"
    }
  }
}

// Customer End

// CustomerCard Start

const CUSTOMER_CARD_STRING_TABLE = {
  URL: {
    GET: {
      POLICIES: "/CustomerCard/GetPolicies"
    }
  }
}

// CustomerCard End

// HotButtonsAndBottleReturn Start

const HOT_BUTTONS_AND_BOTTLE_RETURN_STRING_TABLE = {
  BOTTLE_RETURN_START: 1000,
  NUMBER_OF_HOT_BUTTONS: 36,
  DEFUALT_COLOR: "#00FFFF"
}

// HotButtonsAndBottleReturn End

/*:::::::::::::::: Card Restriction Profile Start ::::::::::::::::*/

const CARD_RESTRICTION_PROFILE_STRING_TABLE = {
  URL: {
    CARD_RESTRICTION_PROFILE_GRID: "/CardRestrictionProfileGrid",
    SAVE_TIME_OF_PURCHASE_RESTRICTION: "CardRestrictionProfile/SaveTimeOfPurchaseRestriction",
    GET_TIME_OF_PURCHASE_RESTRICTION: "CardRestrictionProfile/GetTimeOfPurchaseRestriction",
    GET_ALL_PROFILE_PROMPTS: "CardRestrictionProfile/GetAllProfilePrompts",
    GET_PROMPT_MESSAGE: "CardRestrictionProfile/GetPromptMessageByProfileID",
    GET_PROFILE_PROMPT: "CardRestrictionProfile/GetProfilePromptByPromptSequence",
    GET_CARD_RESTRICTION_PROFILE: "CardRestrictionProfile/GetCardRestrictionProfile",
    DELETE_PROFILE_PROMPT: "CardRestrictionProfile/DeleteProfilePrompt",
    ADD_PROFILE_PROMPT: "CardRestrictionProfile/AddProfilePrompt",
  },
  TABLE: {
    COLUMN_NAME: {
      DAY_OF_WEEK: "dayOfWeek",
      ALLOW_PURCHASE: "allowPurchase",
      TIME_RESTRICTION: "timeRestriction",
      START_TIME: "startTime",
      END_TIME: "endTime",
    },
  },
};

/*:::::::::::::::: Card Restriction Profile End ::::::::::::::::*/

/*:::::::::::::::: Customer Group Start ::::::::::::::::*/

const DISCOUNT_TYPE = {
  PERCENT_DISCOUNT: '%',
  DOLLAR_DISCOUNT: '$',
  COUPON: 'C',
  CASH_BONUS: 'B',
  DISCOUNT_CHART: 'D',
}

/*:::::::::::::::: Customer Group End ::::::::::::::::*/

/*:::::::::::::::: TAX Start ::::::::::::::::*/

const TAX_STRING_TABLE = {
  URL: {
    GET_TAXES: "Tax/GetTaxes",
    GET_POLICIES: "Tax/GetPolicies",
    DELETE_TAX: "Tax/DeleteTax",
    SAVE_TAX: "Tax/SaveTax",
    SAVE_TAX_RATE: "Tax/SaveTaxRate",
    TAX_NAME_EXISTS: "Tax/TaxNameAlreadyExists",
    TAX_CODE_EXISTS: "Tax/TaxCodeAlreadyExists",
    GET_TAX_USAGE_COUNT: "Tax/GetTaxUsageCount",
    GET_TAX_CODE_USAGE_COUNT: "Tax/GetTaxCodeUsageCount",
    GET_TAX_RATES: "Tax/GetTaxRatesByTaxName",
    DELETE_TAX_RATE: "Tax/DeleteTaxRate",
  },
};

/*:::::::::::::::: TAX End ::::::::::::::::*/

/*:::::::::::::::: CHARGE Start ::::::::::::::::*/

const CHARGE_STRING_TABLE = {
  URL: {
    GET_CHARGES: "AssociatedCharges/GetAssociations",
    DELETE_CHARGE: "AssociatedCharges/DeleteAssociation",
    SAVE_CHARGE: "AssociatedCharges/SaveAssociation",
    CHARGE_EXISTS: "AssociatedCharges/IsAssociationExists",
    GET_TAXES: "AssociatedCharges/GetTaxes",
    GET_AVAILABLE_TAXES: "AssociatedCharges/GetAvailableTaxes",
    ADD_TAX: "AssociatedCharges/AddTax",
    DELETE_TAX: "AssociatedCharges/DeleteTax",
    IS_CHARGE_IN_USE: "AssociatedCharges/IsChargeInUse",
  },
};

/*:::::::::::::::: CHARGE End ::::::::::::::::*/

/*:::::::::::::::: CHARGE Start ::::::::::::::::*/

const REGISTERS_STRING_TABLE = {
  URL: {
    GET_REGISTERS: "Registers/GetRegisters",
    GET_REGISTERS_BY_REG_NO: "Registers/GetRegistersByRegNo",
    GET_CUSTOMER_DIPLAY: "Registers/GetCustomerDisplay",
  },
};

/*:::::::::::::::: CHARGE End ::::::::::::::::*/

/*:::::::::::::::: STOCK DESCRIPTION ELEMENTS Start ::::::::::::::::*/

const STOCK_DESCRIPTION_ELEMENTS = {
  DATA_TABLE: {
    BRAND: 'brand', 
    GENERIC: 'generic', 
    CHARACTERISTIC: 'characteristic', 
    FAMILY: 'family', 
    UNIT_OF_MEASURE: 'unitOfMeasure', 
    ORDER_UNIT: 'orderUnit', 
    GROUP: 'group' 
  }
};

/*:::::::::::::::: STOCK DESCRIPTION ELEMENTS End ::::::::::::::::*/

/*:::::::::::::::: TILLS Start ::::::::::::::::*/

const TILLS = {
  CONSTANT_VALUES: {
    TILL90: 90, 
    PAYPUMPTILL: 100, 
    TRAINFIRSTTILL: 91, 
    TRAINLASTTILL: 99 
  }
};

/*:::::::::::::::: TILLS End ::::::::::::::::*/

/*:::::::::::::::: FUEL PRICE CHANGE Start ::::::::::::::::*/

const FUEL_PRICE_CHANGE = {
  GRADE: "grade",
  TIER_AND_LEVEL: "tierAndLevel",
  LOWER_LIMIT: 0, 
  UPPER_LIMIT: 100000.000
}

/*:::::::::::::::: FUEL PRICE CHANGE End ::::::::::::::::*/
