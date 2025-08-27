using Microsoft.Data.Sqlite;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace MemoApp
{
    public partial class MainWindow : Window
    {
        private readonly MemoRepository _repo;
        private Point _dragStartPoint;
        private ObservableCollection<Memo> Memos { get; set; } = [];

        public static RoutedUICommand Save { get; } = 
            new RoutedUICommand("Save", "Save", typeof(MainWindow),
                new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) }
        );

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this; //DataContext が MainWindow 自身。MainWindow の Memos を XAML から使えるようにする
            _repo = new MemoRepository("Data Source=memo.db");

            MemoTitleListBox.ItemsSource = Memos;
            LoadMemos();

            CommandBindings.Add(new CommandBinding(Save, SaveMemoButton_Click));
        }

        private void LoadMemos()
        {
            Memos.Clear();
            foreach (var memo in _repo.GetAll())
            {
                Memos.Add(memo);
            }
        }

        private void SaveMemoButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemoTitleListBox.SelectedItem is Memo selected)
            {
                var id = selected.Id;
                _repo.Update(selected.Id, TitleTextBox.Text, MemoTextBox.Text);
                LoadMemos();
                MemoTitleListBox.SelectedItem = Memos.FirstOrDefault(m => m.Id == id);
            }
            else
            {
                _repo.Save(TitleTextBox.Text, MemoTextBox.Text);
                LoadMemos();
            }
            SaveStatus.Text = "";  //編集中を解除
        }

        private void DeleteMemo_Click(object sender, RoutedEventArgs e)
        {
            if (MemoTitleListBox.SelectedItem is Memo selected)
            {
                _repo.Delete(selected.Id);
                LoadMemos();
                TitleTextBox.Clear();
                MemoTextBox.Clear();
                SaveStatus.Text = "";  //編集中に削除した場合を考慮

            }
        }
        private void NewMemoButton_Click(object sender, RoutedEventArgs e)
        {
            TitleTextBox.Clear();
            MemoTextBox.Clear();
            MemoTitleListBox.SelectedItem = null;
            SaveStatus.Text = "";
        }

        private void MemoTitleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MemoTitleListBox.SelectedItem is Memo selected)
            {
                TitleTextBox.Text = selected.Title;
                MemoTextBox.Text = selected.Content;
                SaveStatus.Text = "";
            }
        }

        private void MemoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveStatus.Text = "編集中";
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveStatus.Text = "編集中";
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void MemoTitleListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(null);
                if (Math.Abs(pos.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(pos.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (MemoTitleListBox.SelectedItem != null)
                    {
                        DragDrop.DoDragDrop(MemoTitleListBox, MemoTitleListBox.SelectedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Memo)))
            {
                var droppedData = e.Data.GetData(typeof(Memo)) as Memo;
                var target = ((FrameworkElement)e.OriginalSource).DataContext as Memo;

                if (droppedData != null && target != null)
                {
                    int removedIdx = Memos.IndexOf(droppedData);
                    int targetIdx = Memos.IndexOf(target);

                    if (removedIdx != targetIdx)
                    {
                        Memos.RemoveAt(removedIdx);
                        Memos.Insert(targetIdx, droppedData);

                        for (int i = 0; i < Memos.Count; i++)
                        {
                            _repo.UpdateOrder(Memos[i].Id, i);
                        }
                    }

                }
            }
        }

    }
}