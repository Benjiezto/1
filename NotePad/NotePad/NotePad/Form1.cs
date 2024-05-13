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
        }
        bool isUndo = false;
        private Stack<string> textHistory = new Stack<string>();
        private const int MaxHistoryCount = 10; // 最多紀錄10個紀錄

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // 設置對話方塊標題
            openFileDialog1.Title = "選擇檔案";
            // 設置對話方塊篩選器，限制使用者只能選擇特定類型的檔案
            openFileDialog1.Filter = "文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";
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

                    // 更為簡單的做法，將檔案內容顯示到 RichTextBox 中
                    string fileContent = File.ReadAllText(selectedFileName);
                    rtbText.Text = fileContent;
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
            saveFileDialog1.Filter = "文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";
            // 如果希望預設儲存的檔案類型是文字檔案，可以這樣設置
            saveFileDialog1.FilterIndex = 1;
            // 如果希望對話方塊在開啟時顯示的初始目錄，可以設置 InitialDirectory
            saveFileDialog1.InitialDirectory = "D:\\work\\視窗程式\\NotePad";

            // 顯示對話方塊，並等待使用者指定儲存的檔案
            DialogResult result = saveFileDialog1.ShowDialog();

          

            if (result == DialogResult.OK)
            {
                try
                {
                    // 使用者指定的儲存檔案的路徑
                    string saveFileName = saveFileDialog1.FileName;

                    // 將 RichTextBox 中的文字儲存到檔案中
                    File.WriteAllText(saveFileName, rtbText.Text);

                    MessageBox.Show("檔案儲存成功。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // 如果發生錯誤，用 MessageBox 顯示錯誤訊息
                    MessageBox.Show("儲存檔案時發生錯誤: " + ex.Message, "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    
                }
            }
            else
            {
                MessageBox.Show("使用者取消了儲存檔案操作。", "訊息", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            isUndo = true;
            if (textHistory.Count > 1)
            {
                textHistory.Pop(); // 移除當前的文本內容
                rtbText.Text = textHistory.Peek(); // 將堆疊頂部的文本內容設置為當前的文本內容                
            }
            UpdateListBox();
            isUndo = false;
        }

        private void rtbText_TextChanged(object sender, EventArgs e)
        {
            if (!isUndo)
            {
                // 將當前的文本內容加入堆疊
                textHistory.Push(rtbText.Text);

                // 確保堆疊中只保留最多10個紀錄
                if (textHistory.Count > MaxHistoryCount)
                {
                    // 移除最底下的一筆資料
                    Stack<string> tempStack = new Stack<string>();
                    for (int i = 0; i < MaxHistoryCount; i++)
                    {
                        tempStack.Push(textHistory.Pop());
                    }
                    textHistory.Pop(); // 移除最底下的一筆資料
                    foreach (string item in tempStack)
                    {
                        textHistory.Push(item);
                    }
                }
                UpdateListBox(); // 更新 ListBox
            }
            void UpdateListBox()
            {
                listUndo.Items.Clear(); // 清空 ListBox 中的元素

                // 將堆疊中的內容逐一添加到 ListBox 中
                foreach (string item in textHistory)
                {
                    listUndo.Items.Add(item);
                }
            }
        }
    }
}