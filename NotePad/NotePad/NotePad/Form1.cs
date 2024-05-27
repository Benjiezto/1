using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;    // 使用 IO 函式庫

namespace NotePad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitializeFontComboBox();
            InitializeFontSizeComboBox();
            InitializeFontStyleComboBox();
        }
        private bool isUndoRedo = false;                           // 是否在回復或重作階段
        private Stack<MemoryStream> undoStack = new Stack<MemoryStream>(); // 回復堆疊
        private Stack<MemoryStream> redoStack = new Stack<MemoryStream>(); // 重作堆疊
        private const int MaxHistoryCount = 10;

        private int selectionStart = 0;                            // 記錄文字反白的起點
        private int selectionLength = 0;                           // 記錄文字反白的長度

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // 設置對話方塊標題
            openFileDialog1.Title = "選擇檔案";
            // 設置對話方塊篩選器，限制使用者只能選擇特定類型的檔案
            openFileDialog1.Filter = "RTF格式檔案 (*.rtf)|*.rtf|文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";
            // 如果希望預設開啟的檔案類型是文字檔案，可以這樣設置
            openFileDialog1.FilterIndex = 1;
            // 如果希望對話方塊在開啟時顯示的初始目錄，可以設置 InitialDirectory
            openFileDialog1.InitialDirectory = "D:\\work\\視窗程式\\NotePad";
            // 允許使用者選擇多個檔案
            openFileDialog1.Multiselect = true;

            // 顯示對話方塊，並等待使用者選擇檔案
            DialogResult result = openFileDialog1.ShowDialog();

            // 檢查使用者是否選擇了檔案
            if (result == DialogResult.OK)
            {
                try
                {
                    // 使用者在OpenFileDialog選擇的檔案
                    string selectedFileName = openFileDialog1.FileName;

                    // 獲取文件的副檔名
                    string fileExtension = Path.GetExtension(selectedFileName).ToLower();

                    // 判斷副檔名是甚麼格式
                    if (fileExtension == ".txt")
                    {
                        // 使用 using 與 FileStream 打開檔案
                        using (FileStream fileStream = new FileStream(selectedFileName, FileMode.Open, FileAccess.Read))
                        {
                            // 使用 StreamReader 讀取檔案內容
                            using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                            {
                                // 將檔案內容顯示到 RichTextBox 中
                                rtbText.Text = streamReader.ReadToEnd();
                            }
                        }
                    }
                    else if (fileExtension == ".rtf")
                    {
                        // 如果是RTF文件，使用RichTextBox的LoadFile方法
                        rtbText.LoadFile(selectedFileName, RichTextBoxStreamType.RichText);
                    }

                }
                catch (Exception ex)
                {
                    // 如果發生錯誤，用MessageBox顯示錯誤訊息
                    MessageBox.Show("讀取檔案時發生錯誤: " + ex.Message, "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("使用者取消了選擇檔案操作。", "訊息", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 設置對話方塊標題
            saveFileDialog1.Title = "儲存檔案";
            // 設置對話方塊篩選器，限制使用者只能選擇特定類型的檔案
            saveFileDialog1.Filter = "RTF格式檔案 (*.rtf)|*.rtf|文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";
            // 如果希望預設儲存的檔案類型是文字檔案，可以這樣設置
            saveFileDialog1.FilterIndex = 1;
            // 如果希望對話方塊在開啟時顯示的初始目錄，可以設置 InitialDirectory
            saveFileDialog1.InitialDirectory = "D:\\work\\視窗程式\\NotePad";

            // 顯示對話方塊，並等待使用者指定儲存的檔案
            DialogResult result = saveFileDialog1.ShowDialog();



            FileStream fileStream = null;

            // 檢查使用者是否選擇了檔案
            if (result == DialogResult.OK)
            {
                try
                {
                    // 使用者指定的儲存檔案的路徑
                    string saveFileName = saveFileDialog1.FileName;
                    string extension = Path.GetExtension(saveFileName);

                    // 使用 using 與 FileStream 建立檔案，如果檔案已存在則覆寫
                    using (fileStream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
                    {
                        if (extension.ToLower() == ".txt")
                        {
                            // 將 RichTextBox 中的文字寫入檔案中
                            byte[] data = Encoding.UTF8.GetBytes(rtbText.Text);
                            fileStream.Write(data, 0, data.Length);
                        }
                        else if (extension.ToLower() == ".rtf")
                        {
                            // 將RichTextBox中的內容保存為RTF格式
                            rtbText.SaveFile(fileStream, RichTextBoxStreamType.RichText);
                        }
                    }

                    MessageBox.Show("檔案儲存成功。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // 如果發生錯誤，用 MessageBox 顯示錯誤訊息
                    MessageBox.Show("儲存檔案時發生錯誤: " + ex.Message, "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // 關閉資源，如果使用 using 或者直接以 File.WriteAllText 儲存文字檔，可以不需要。
                    fileStream.Close();
                }
            }
            else
            {
                MessageBox.Show("使用者取消了儲存檔案操作。", "訊息", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 1)
            {
                isUndoRedo = true;
                redoStack.Push(undoStack.Pop()); // 將回復堆疊最上面的紀錄移出，再堆到重作堆疊
                MemoryStream lastSavedState = undoStack.Peek(); // 將回復堆疊最上面一筆紀錄顯示
                LoadFromMemory(lastSavedState);
                UpdateListBox();
                isUndoRedo = false;
            }
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                isUndoRedo = true;
                undoStack.Push(redoStack.Pop()); // 將重作堆疊最上面的紀錄移出，再堆到回復堆疊
                MemoryStream lastSavedState = undoStack.Peek(); // 將回復堆疊最上面一筆紀錄顯示
                LoadFromMemory(lastSavedState);
                UpdateListBox();
                isUndoRedo = false;
            }
        }

        private void rtbText_TextChanged(object sender, EventArgs e)
        {
            // 只有當isUndo這個變數是false的時候，才能堆疊文字編輯紀錄
            if (isUndoRedo == false)
            {
                SaveCurrentStateToStack(); // 將當前的文本內容加入堆疊
                redoStack.Clear();            // 清空重作堆疊

                // 確保堆疊中只保留最多10個紀錄
                if (undoStack.Count > MaxHistoryCount)
                {
                    // 用一個臨時堆疊，將除了最下面一筆的文字記錄之外，將文字紀錄堆疊由上而下，逐一移除再堆疊到臨時堆疊之中
                    Stack<MemoryStream> tempStack = new Stack<MemoryStream>();
                    for (int i = 0; i < MaxHistoryCount; i++)
                    {
                        tempStack.Push(undoStack.Pop());
                    }
                    undoStack.Clear(); // 清空堆疊
                                       // 文字編輯堆疊紀錄清空之後，再將暫存堆疊（tempStack）中的資料，逐一放回到文字編輯堆疊紀錄
                    foreach (MemoryStream item in tempStack)
                    {
                        undoStack.Push(item);
                    }
                }
                UpdateListBox(); // 更新 ListBox
            }
        }
            void UpdateListBox()
            {
                listUndo.Items.Clear(); // 清空 ListBox 中的元素

                // 將堆疊中的內容逐一添加到 ListBox 中
                foreach (MemoryStream item in undoStack)
                {
                    listUndo.Items.Add(item);
                }
            }

        private void InitializeFontComboBox()
        {
            foreach (FontFamily font in FontFamily.Families)
            {
                comboBoxFont.Items.Add(font.Name);
            }
            comboBoxFont.SelectedIndex = 0;
        }

        private void InitializeFontSizeComboBox()
        {
            for (int i = 8; i <= 72; i += 2)
            {
                comboBoxSize.Items.Add(i);
            }
            comboBoxSize.SelectedIndex = 2;
        }

        private void InitializeFontStyleComboBox()
        {
            comboBoxStyle.Items.Add(FontStyle.Regular.ToString());
            comboBoxStyle.Items.Add(FontStyle.Bold.ToString());
            comboBoxStyle.Items.Add(FontStyle.Italic.ToString());
            comboBoxStyle.Items.Add(FontStyle.Underline.ToString());
            comboBoxStyle.Items.Add(FontStyle.Strikeout.ToString());
            comboBoxStyle.SelectedIndex = 0;
        }


        private void comboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rtbText.SelectionFont != null)
            {
                // 確保選擇的字型、大小和樣式都不為 null
                string selectedFont = comboBoxFont.SelectedItem?.ToString();
                string selectedSizeStr = comboBoxSize.SelectedItem?.ToString();
                string selectedStyleStr = comboBoxStyle.SelectedItem?.ToString();

                if (selectedFont != null && selectedSizeStr != null && selectedStyleStr != null)
                {
                    float selectedSize = float.Parse(selectedSizeStr);
                    FontStyle selectedStyle = (FontStyle)Enum.Parse(typeof(FontStyle), selectedStyleStr);

                    // 獲取當前選擇的文字的字型和樣式
                    Font currentFont = rtbText.SelectionFont;
                    FontStyle newStyle = currentFont.Style;

                    // 檢查是否需要應用新的樣式
                    if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Bold.ToString())
                        newStyle = FontStyle.Bold;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Italic.ToString())
                        newStyle = FontStyle.Italic;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Underline.ToString())
                        newStyle = FontStyle.Underline;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Strikeout.ToString())
                        newStyle = FontStyle.Strikeout;
                    else
                        newStyle = FontStyle.Regular;

                    Font newFont = new Font(selectedFont, selectedSize, newStyle);
                    rtbText.SelectionFont = newFont;
                }
            }
            // 恢復選擇狀態
            rtbText.Focus();
            rtbText.Select(selectionStart, selectionLength);
        }
        // 將文字編輯狀態保存到回復堆疊
        private void SaveCurrentStateToStack()
        {
            // 創建一個新的 MemoryStream 來保存文字編輯狀態
            MemoryStream memoryStream = new MemoryStream();
            // 將 RichTextBox 的內容保存到 memoryStream
            rtbText.SaveFile(memoryStream, RichTextBoxStreamType.RichText);
            // 將 memoryStream 放入回復堆疊
            undoStack.Push(memoryStream);
        }

        // 將文字狀態從記憶體中顯示到 RichTextBox
        private void LoadFromMemory(MemoryStream memoryStream)
        {
            // 將 memoryStream 的指標重置到開始位置
            memoryStream.Seek(0, SeekOrigin.Begin);
            // 將 memoryStream 的內容放到到 RichTextBox
            rtbText.LoadFile(memoryStream, RichTextBoxStreamType.RichText);
        }
    }

     
    }
