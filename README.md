#測試數據處理工具

## 📋 工具概述

**Xml上拋**是一款 C# Windows Forms 應用程式，針對半導體測試設備的數據處理和管理，提供多功能的 XML、CSV 和 STDF 檔案處理功能。

---

## 🗂️ 功能分頁說明

### **分頁 1：Xml上拋**
- **功能**：修改和上傳 XML 配置文件
- **主要操作**：
  - 修改 BARCODE_SYSTEM 節點中的參數：LBC1/LBC2、RRT_flag、RT_Bin、RT_Frequency
  - 支援選擇性啟用各項參數修改
  - 同時上傳到多個 FTP/本地路徑（j750_summary 和 barcode_backup）
  - 提供 FTP 和本地路徑切換選項
  - 機台名稱和狀態顯示
- **按鈕**：
  - 上拋Xml：執行 XML 修改和上傳
  - 程式所在位置：打開執行檔目錄

---

### **分頁 2：.Sum下載**
- **功能**：從 FTP 伺服器下載測試結果檔案並壓縮
- **主要操作**：
  - 輸入 RN（Run Number）和選擇測試站點（FT1-FT5）
  - 自動篩選符合條件的檔案（包含 RunCard 和站點名稱、排除 CORR 和 HW）
  - 支援多個 FTP 伺服器路徑配置（L401、L176）
  - 將下載的檔案打包成 ZIP
  - 自動清理臨時檔案
- **按鈕**：
  - Download FTP File：執行下載和壓縮
  - 程式所在位置：打開執行檔目錄

---

### **分頁 3：F278 Rename**
- **功能**：重命名 F278 測試設備生成.std.gz壓縮檔案
- **主要操作**：
  - 支援 FT（Final Test）和 CP（Component Program）兩種測試類型
  - 輸入 DateCode 和 Tester 資訊
  - 解壓縮 .gz 檔案 → 重命名 → 重新壓縮
  - 自動提取檔名中的時間戳和參數信息
  - 最終生成標準命名格式的 .std.gz 檔案
- **按鈕**：
  - Rename：執行重命名操作
  - 程式根目錄：打開執行檔目錄

---

### **分頁 4：Xml_Reader**
- **功能**：解析 XML 檔案並匯出到 CSV
- **主要操作**：
  - 支援兩種 XML 類型：SECN 規格書和委測需求單
  - 自動偵測多種編碼（UTF-8、UTF-16、Big5）
  - 轉義 XML 內容中的特殊字符（<、>）
  - 依 Product 分組並按 Location 排序
  - 輸出格式化的 CSV 檔案（支援 UTF-8 和 Big5 編碼）
  - 進行重複檢測和異常記錄
- **按鈕**：
  - 讀取：選擇資料夾並執行 XML 解析
  - 程式所在位置：打開執行檔目錄

---

### **分頁 5：HandlerLog分析**
- **功能**：整合和分析生產日誌數據
- **主要操作**：
  - **ProductionLog彙整排序**：
    - 解析複雜的 CSV 日誌檔案（39 欄位）
    - 按測試順序分組，組內按站點編號排序
    - 輸出排序後的 TrayMap_Sorted.csv
  - **STDF_CSV彙整**：
    - 提取和轉換 STDF 測試數據
    - 動態識別 CSV 欄位位置
    - 輸出標準化的 STDF_csv.csv
  - **兩份檔案彙整**：
    - 將 TrayMap 和 STDF 資料進行序列對齊
    - 依據 TestCategory 和 HBIN 自動尋找對齊點
    - 合併輸出 Merged_Aligned.csv（58 欄位）
- **按鈕**：
  - ProductionLog彙整排序：分析和排序 HandlerLog
  - STDF_CSV彙整：轉換 STDF 格式
  - 兩份檔案彙整：合併 TrayMap 和 STDF 數據
  - 程式所在位置：打開執行檔目錄

---

## ⚙️ 配置說明

### 配置文件（config.ini）
- **MTK_auto_load_E750**：J750 測試機 FTP 設定
  - Ftp、Account、Password
- **barcode_backup**：條碼備份路徑設定
  - Ftp、Account、Password

### 支援路徑
- **一般路徑**：本地網路共享路徑（預設 172.17.6.212、172.17.6.192）
- **FTP 路徑**：遠端 FTP 伺服器配置

---

## 🛠️ 技術堆棧

- **框架**：.NET Framework / Windows Forms
- **語言**：C# 7.3+
- **主要套件**：
  - FluentFTP：FTP 操作
  - System.Xml：XML 處理
  - System.IO.Compression：ZIP 壓縮
- **編碼支援**：UTF-8、UTF-16、Big5/CP950

---

## 📝 使用場景

1. 為半導體測試設備管理 XML 配置文件
2. 從多台測試機自動收集測試結果
3. 統一管理和轉換測試數據格式
4. 進行測試數據的對齊和交叉驗證
5. 生成標準化的生產日誌報告

---

## 📌 注意事項

- 執行前確保 config.ini 在程式根目錄
- FTP 路徑需正確配置認證信息
- XML 檔案應遵循特定的 DTD 結構
- CSV 輸出預設使用 UTF-8 編碼（可選 Big5）
- 大型檔案處理可能需要較長時間
