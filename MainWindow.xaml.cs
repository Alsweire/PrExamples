using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Serialization;
using XamlReader = System.Windows.Markup.XamlReader;
using Point = System.Windows.Point;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls.Primitives;

namespace WpfApp1
{
    public static class Request
    {
        public static string _name = "";
        public static int _semester = 0;
        public static string _comment = "";
    }

   /* public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();
            textB1.Text = Request._name;
            comb.ItemsSource = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            comb.SelectedValue = Request._semester;
            textB3.Text = Request._comment;
        }

        public void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Request._name = textB1.Text;
            Request._semester = (int)comb.SelectedValue;
            Request._comment = textB3.Text;
            this.Close();
            
        }

        public void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }*/
    public class Discipline
    {
        public string Name { get; set; }
        public int Semester { get; set; }
        public List<Discipline> Ancestors = new List<Discipline>();
        public List<Discipline> Children = new List<Discipline>();
        public string Comment { get; set; }
        public Boolean BeingAdded = false;

        public Discipline(string Name, int Semester, string Comment)
        {
            this.Name = Name;   
            this.Semester= Semester;
            this.Comment = Comment;
        }

        
        public String ToString()
        {
            string list1 = "";
            string list2 = "";
            for (int i = 0; i < this.Ancestors.Count; i++)
            {
                list1 += " • " + this.Ancestors[i].Name + "\n";
            }

            for (int i = 0; i < this.Children.Count; i++)
            {
                list2 += " • " + this.Children[i].Name + "\n";
            }

            if (list1.Equals(""))
            {
                list1 = "Отсутствуют";
            }

            if (list2.Equals(""))
            {
                list2 = "Отсутствуют";
            }

            return "Название дисциплины: " + Name +
                "\nСеместр:  " + Convert.ToString(Semester) +
                "\nПредшествующие дисциплины:\n" + list1 +
                "\nПоследующие дисциплины:\n" + list2 +
                "\nКомментарий: " + Comment;
        }

        public Boolean Equals(Discipline other)
        {
            if (other.Name.Equals(this.Name)) return true;
            else return false;
        }

    }

    public class CoordSpline
    {
        public double X_Start;
        public double Y_Start;
        public double X_End;
        public double Y_End;
        public double X_Middle;
        public double Y_Middle;
        public Discipline StartDiscipline;
        public Discipline EndDiscipline;
        public CoordSpline(double X_Start, double Y_Start, double X_Middle, double Y_Middle, double X_End, double Y_End, Discipline StartDiscipline, Discipline EndDiscipline)
        {
            this.X_Start = X_Start;
            this.Y_Start = Y_Start;
            this.X_Middle= X_Middle;        
            this.Y_Middle= Y_Middle;
            this.X_End= X_End;
            this.Y_End= Y_End;
            this.StartDiscipline = StartDiscipline;
            this.EndDiscipline = EndDiscipline;
        }
    }

    public class TextEllipse
    {
        public Ellipse Ell;
        public TextBlock Textbl;
        public TextEllipse(Ellipse Ell, TextBlock Textbl)
        { 
            this.Ell = Ell;
            this.Textbl = Textbl;
        }

    }

    public class Coordinates
    {
        public double X;
        public double Y;
        public Coordinates(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public class CurvedLine
    {
        public Path Line;
        public Discipline StartDiscipline;
        public Discipline EndDiscipline;
        public CurvedLine(Path Line, Discipline StartDiscipline, Discipline EndDiscipline)
        { 
            this.Line = Line;
            this.StartDiscipline = StartDiscipline;
            this.EndDiscipline = EndDiscipline;
        }
    }

    //основные виндоусы

    public partial class MainWindow : Window
    {
        ObservableCollection<Discipline> disciplines = new ObservableCollection<Discipline>();
        List<TextEllipse> ellipses = new List<TextEllipse>();
        List<CoordSpline> splines = new List<CoordSpline>();
        List<Coordinates> coords = new List<Coordinates>();
        List<CurvedLine> lines = new List<CurvedLine>();
        Boolean stateOfGraph = false;
        public MainWindow()
        {
            InitializeComponent();
            Semester.SelectedIndex = 0;
            DeletingDiscipline.ItemsSource = GetNames(disciplines);
            Discipline1Relation.ItemsSource = GetNames(disciplines);
            Discipline2Relation.ItemsSource = GetNames(disciplines);
            DrawOrientedGraph();

        }
        //Меню

        private void NewFile_Click(object sender, RoutedEventArgs e)
        { 
            //создание нового файла бд
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            //открытие нового файла бд
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            disciplines.Clear();
        }

        private void Instruction(object sender, RoutedEventArgs e)
        {
            //справка
        }

        //Кнопочки запросов
        private void ButtonCreateDisciplineClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)canvas.Children[i];
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    canvas.Children[i] = ellipse;
                }
            }

            TextBlock tb = new TextBlock();
            tb.Text = "Область для вывода информации о дисциплине";
            TextSpace.Content = tb;

            string text = DisciplineName.Text;
            string semester = "0";
            if (Semester.SelectedValue.ToString() == null || Semester.SelectedValue.ToString() == "-")
            {
                semester = "0";
            }
            else
            {
                semester = Semester.SelectedValue.ToString();
            }

            if (Regex.IsMatch(text, @"^\w+[\w\s-]*\w+$") && semester != null)
            {
                int sem = Convert.ToInt32(semester.Substring(0, 1));
                string comment = CommentBox.Text.ToString();
                if (comment.Equals(null))
                {
                    comment = "";
                }

                if (GetNames(disciplines).Contains(text) == false)
                {
                    disciplines.Add(new Discipline(text, sem, comment));
                    DisciplineName.Text = String.Empty;
                    CommentBox.Text = String.Empty;
                    Discipline1Relation.ItemsSource = GetNames(disciplines);
                    Discipline2Relation.ItemsSource = GetNames(disciplines);
                    DeletingDiscipline.ItemsSource = GetNames(disciplines);
                    WriteOrientedGraph();
                }
                else
                {
                    MessageBox.Show("Дисциплина с таким названием уже существует!");
                }
            }
            else
            {
                MessageBox.Show("Название содержит другие символы, помимо букв и цифр!");
            }
        }
        private void DisciplineNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                for (int i = 0; i < canvas.Children.Count; i++)
                {
                    if (canvas.Children[i] is Ellipse)
                    {
                        Ellipse ellipse = (Ellipse)canvas.Children[i];
                        ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                        canvas.Children[i] = ellipse;
                    }
                }

                TextBlock tb = new TextBlock();
                tb.Text = "Область для вывода информации о дисциплине";
                TextSpace.Content = tb;

                string text = DisciplineName.Text;
                string semester = "0";
                if (Semester.SelectedValue.ToString() == null || Semester.SelectedValue.ToString() == "-")
                {
                    semester = "0";
                }
                else
                {
                    semester = Semester.SelectedValue.ToString();
                }

                if (Regex.IsMatch(text, @"^\w+[\w\s-]*\w+$") && semester != null)
                {
                    int sem = Convert.ToInt32(semester.Substring(0, 1));
                    string comment = CommentBox.Text.ToString();
                    if (comment.Equals(null))
                    {
                        comment = "";
                    }

                    if (GetNames(disciplines).Contains(text) == false)
                    {
                        disciplines.Add(new Discipline(text, sem, comment));
                        DisciplineName.Text = String.Empty;
                        CommentBox.Text = String.Empty;
                        Discipline1Relation.ItemsSource = GetNames(disciplines);
                        Discipline2Relation.ItemsSource = GetNames(disciplines);
                        DeletingDiscipline.ItemsSource = GetNames(disciplines);
                        WriteOrientedGraph();
                    }
                    else
                    {
                        MessageBox.Show("Дисциплина с таким названием уже существует!");
                    }
                }
                else
                {
                    MessageBox.Show("Название содержит другие символы, помимо букв и цифр!");
                }

            }
        }

        private void ButtonDeleteDisciplineClick(object senser, RoutedEventArgs e)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)canvas.Children[i];
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    canvas.Children[i] = ellipse;
                }
            }

            TextBlock tb = new TextBlock();
            tb.Text = "Область для вывода информации о дисциплине";
            TextSpace.Content = tb;

            if (DeletingDiscipline.SelectedValue != null) {
                string text = DeletingDiscipline.SelectedValue.ToString();
                if (text == null)
                {
                    MessageBox.Show("Не выбрана дисциплина для удаления!");
                }
                else
                {
                    Discipline temp = GetDisciplineByName(text);
                    for (int i = 0; i < temp.Ancestors.Count; i++)
                    {
                        disciplines[GetIndexOf(temp.Ancestors[i])].Children.Remove(temp);
                    }

                    for (int i = 0; i < temp.Children.Count; i++)
                    {
                        disciplines[GetIndexOf(temp.Children[i])].Ancestors.Remove(temp);
                    }

                    if (GetNames(disciplines).Contains(text))
                    {
                        Ellipse ellipse = ellipses[GetIndexOfEll(disciplines[GetIndexOf(temp)].Name)].Ell;
                        int k = 0;
                        if (lines.Count > 0)
                        {
                            while (k == 0)
                            {
                                for (int i = 0; i < lines.Count; i++)
                                {
                                    if (lines[i].StartDiscipline == temp || lines[i].EndDiscipline == temp)
                                    {
                                        if (i == lines.Count - 1)
                                        {
                                            lines.RemoveAt(i);
                                            splines.RemoveAt(i);
                                            k = 1;
                                        }
                                        else
                                        {
                                            lines.RemoveAt(i);
                                            splines.RemoveAt(i);
                                            break;
                                        }
                                    }
                                    else if (i == lines.Count - 1)
                                    {
                                        k = 1;
                                    }
                                }
                            }
                        }

                        coords.RemoveAt(GetIndexOfCoord(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse)));
                        ellipses.RemoveAt(GetIndexOfEll(disciplines[GetIndexOf(temp)].Name));
                        disciplines.RemoveAt(GetIndexOf(temp));
                        Discipline1Relation.ItemsSource = GetNames(disciplines);
                        Discipline2Relation.ItemsSource = GetNames(disciplines);
                        DeletingDiscipline.ItemsSource = GetNames(disciplines);
                        if (stateOfGraph == false)
                        {
                            DrawOrientedGraph();
                        }
                        else
                        {
                            ShowData();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Такой дисциплины нет!");
                    }
                }}
           
        }

        private void ButtonCreateRelationClick(object senser, RoutedEventArgs e)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)canvas.Children[i];
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    canvas.Children[i] = ellipse;
                }
            }

            TextBlock tb = new TextBlock();
            tb.Text = "Область для вывода информации о дисциплине";
            TextSpace.Content = tb;


            if (Discipline1Relation.SelectedValue != Discipline2Relation.SelectedValue && Discipline1Relation.SelectedValue != null && Discipline2Relation.SelectedValue != null)
            {
                Discipline temp1 = GetDisciplineByName(Discipline1Relation.SelectedValue.ToString());
                Discipline temp2 = GetDisciplineByName(Discipline2Relation.SelectedValue.ToString());
                if (temp1 != null && temp2 != null)
                {
                    if (disciplines[disciplines.IndexOf(temp1)].Children.Contains(temp2) == false && disciplines[disciplines.IndexOf(temp2)].Ancestors.Contains(temp1) == false)
                    {
                        if (temp1.Semester <= temp2.Semester)
                        {
                            disciplines[disciplines.IndexOf(temp1)].Children.Add(temp2);
                            disciplines[disciplines.IndexOf(temp2)].Ancestors.Add(temp1);
                            DrawCurvedLines();
                        }
                        else
                        {
                            MessageBox.Show("Дисциплина " + temp1.Name +" не может идти впереди дисциплины " + temp2.Name +"!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Такое отношение уже есть!");
                    }
                }
                else
                {
                    MessageBox.Show("Непредвиденная ошибка!");
                }
            }
            else
            {
                MessageBox.Show("Отношение не может быть между одной и той же дисциплиной!");
            }
        }

        private void ButtonDeleteRelationClick(object senser, RoutedEventArgs e)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)canvas.Children[i];
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    canvas.Children[i] = ellipse;
                }
            }

            TextBlock tb = new TextBlock();
            tb.Text = "Область для вывода информации о дисциплине";
            TextSpace.Content = tb;

            if (Discipline1Relation.SelectedValue != Discipline2Relation.SelectedValue && Discipline1Relation.SelectedValue != null && Discipline2Relation.SelectedValue != null)
            {
                Discipline temp1 = GetDisciplineByName(Discipline1Relation.SelectedValue.ToString());
                Discipline temp2 = GetDisciplineByName(Discipline2Relation.SelectedValue.ToString());
                if (temp1 != null && temp2 != null && disciplines[disciplines.IndexOf(temp1)].Children.Contains(temp2) && disciplines[disciplines.IndexOf(temp2)].Ancestors.Contains(temp1))
                {
                    lines.RemoveAt(GetIndexOfLine(temp1, temp2));
                    splines.RemoveAt(GetIndexOfSpline(temp1, temp2));
                    disciplines[disciplines.IndexOf(temp1)].Children.Remove(temp2);
                    disciplines[disciplines.IndexOf(temp2)].Ancestors.Remove(temp1);
                    if (stateOfGraph == false)
                    {
                        DrawOrientedGraph();
                    }
                    else
                    {
                        ShowData();
                    }
                }
                else
                {
                    MessageBox.Show("Попытка удалить несуществующее отношение!");
                }
            }
            else
            {
                MessageBox.Show("Отношение не может быть между одной и той же дисциплиной!");
            }

        }

        private void ChangeToList_Click(object sender, RoutedEventArgs e)
        {
            if (stateOfGraph == false)
            {
                //canvas.Children.Clear();
                //for (int i = 0; i < disciplines.Count; i++)
                //{
                //    disciplines[i].BeingAdded = false;
                //}

                if (CheckTheRightnessOfSorting())
                {
                    stateOfGraph = true;
                    canvas.Children.Clear();
                    ShowData();
                }
                
            }
        }

        private void ChangeToGraph_Click(object sender, RoutedEventArgs e)
        {
            if (stateOfGraph == true)
            {
                canvas.Children.Clear();
                //for (int i = 0; i < disciplines.Count; i++)
                //{
                //    disciplines[i].BeingAdded = false;
                //}

                stateOfGraph = false;
                DrawOrientedGraph();
            }
        }


        //какие-то штуки

        private String[] GetNames(ObservableCollection<Discipline> listOfNames)
        { 
            int length = listOfNames.Count;
            String[] names = new String[length];
            for (int i = 0; i < length; i++)
            {
                names[i] = listOfNames[i].Name;
            }

            return names;
        }

        private Discipline GetDisciplineByName(String name)
        {
            for (int i = 0; i < disciplines.Count(); i++)
            {
                if ((disciplines[i].Name).Equals(name))
                {
                    return disciplines[i];
                }
            }
            return null;
        }

        private Boolean Contains(Discipline discipline)
        {
            for (int i = 0; i < disciplines.Count(); i++)
            {
                if (disciplines[i].Equals(discipline))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetIndexOf(Discipline discipline)
        {
            for (int i = 0; i < disciplines.Count(); i++)
            {
                if (disciplines[i].Equals(discipline))
                {
                    return i;
                }
            }

            return -1;
        }

        private Boolean CheckCoords(double X, double Y)
        { 
            for (int i = 0; i < coords.Count(); i++)
            {
                if (((X < coords[i].X + 70) && (X > coords[i].X - 70)) && ((Y > coords[i].Y - 70) && (Y < coords[i].Y + 70)))
                {
                    return false;
                }
            }

            return true;
        }

        private double FindMaxWidth()
        {
            double maxWidth = 0;
            for (int i = 0; i < coords.Count; i++)
            {
                if (coords[i].X > maxWidth)
                {
                    maxWidth = coords[i].Y;
                }
            }

            return maxWidth + 70;
        }
        
        private double FindMaxHeight()
        { 
            double maxHeight = 0;
            for (int i = 0; i < coords.Count; i++)
            {
                if (coords[i].Y > maxHeight)
                {
                    maxHeight = coords[i].Y;
                }
            }

            return maxHeight + 70;
        }

        private int GetIndexOfCoord(double x, double y)
        {
            for (int i = 0; i < coords.Count; i++)
            {
                if (coords[i].X == x && coords[i].Y == y)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfEll(string name)
        { 
            for (int i = 0; i < ellipses.Count; i++)
            {
                if (ellipses[i].Textbl.Text.Equals(name))
                {
                    return i;
                }
            }

            return -1;
        }

        private Boolean CheckTheRelationship(Discipline dis1, Discipline dis2)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i].StartDiscipline.Name.Equals(dis1.Name) && splines[i].EndDiscipline.Name.Equals(dis2.Name))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetIndexOfLine(Discipline disc)
        { 
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartDiscipline.Name.Equals(disc.Name) || lines[i].EndDiscipline.Name.Equals(disc.Name))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfSpline(Discipline disc)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i].StartDiscipline.Name.Equals(disc.Name) || splines[i].EndDiscipline.Name.Equals(disc.Name))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfLine(Discipline disc1, Discipline disc2)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartDiscipline.Name.Equals(disc1.Name) && lines[i].EndDiscipline.Name.Equals(disc2.Name))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfSpline(Discipline disc1, Discipline disc2)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i].StartDiscipline.Name.Equals(disc1.Name) && splines[i].EndDiscipline.Name.Equals(disc2.Name))
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfEll(Ellipse ellipse)
        {
            for (int i = 0; i < ellipses.Count; i++)
            {
                if (ellipses[i].Ell.Equals(ellipse))
                {
                    return i;
                }
            }

            return -1;
        }

        //В И З У А Л И З А Ц И Я
        private void WriteOrientedGraph()
        {
            Random rand = new Random();
            for (int i = 0; i < disciplines.Count; i++)
            {
                if (disciplines[i].BeingAdded == false)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Width = 50;
                    ellipse.Height = 50;
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    ellipse.MouseLeftButtonDown += EllipseClickEvent;
                    ContextMenu cm = new ContextMenu();
                    MenuItem mi = new MenuItem();
                    mi.Header = "Редактировать";
                    mi.Click += EditClick;
                    cm.Items.Add(mi);
                    MenuItem mi2 = new MenuItem();
                    mi2.Header = "Удалить";
                    mi2.Click += DeleteClickContextMenu;
                    cm.Items.Add(mi2);
                    ellipse.ContextMenu = cm;
                    TextBlock textbl = new TextBlock();
                    textbl.Background = new SolidColorBrush(Colors.Transparent);
                    textbl.TextWrapping = TextWrapping.Wrap;
                    textbl.TextAlignment = TextAlignment.Center;
                    textbl.Width = 75;
                    textbl.Text = disciplines[i].Name;
                    double top = 0;
                    double left = 0;
                    int k = 0;
                    while (k == 0)
                    {
                        top = rand.Next(disciplines.Count * 70) + 10;
                        left = rand.Next(disciplines.Count * 70) + 10;
                        if (CheckCoords(left, top))
                        {
                            k = 1;
                        }
                    }

                    Canvas.SetTop(ellipse, top);
                    Canvas.SetLeft(ellipse, left);
                    Canvas.SetTop(textbl, top + 15);
                    Canvas.SetLeft(textbl, left - 10);
                    ellipses.Add(new TextEllipse(ellipse, textbl));
                    coords.Add(new Coordinates(left, top));
                    disciplines[i].BeingAdded = true;
                }
            }

            canvas.Width = FindMaxWidth();
            canvas.Height = FindMaxHeight();
            if (stateOfGraph == false)
            {
                DrawOrientedGraph();
            }
            else if (CheckTheRightnessOfSorting())
            {
                ShowData();
            }
            else
            {
                stateOfGraph = false;
                DrawOrientedGraph();
            }
        }

        private void DrawCurvedLines()
        {
            for (int i = 0; i < disciplines.Count; i++)
            {
                for (int j = 0; j < disciplines[i].Children.Count; j++)
                {
                    if (CheckTheRelationship(disciplines[i], disciplines[i].Children[j]) == false)
                    {
                        double X1 = Canvas.GetLeft(ellipses[GetIndexOfEll(disciplines[i].Name)].Ell) + 25;
                        double Y1 = Canvas.GetTop(ellipses[GetIndexOfEll(disciplines[i].Name)].Ell) + 25;
                        double X2 = Canvas.GetLeft(ellipses[GetIndexOfEll(disciplines[i].Children[j].Name)].Ell) + 25;
                        double Y2 = Canvas.GetTop(ellipses[GetIndexOfEll(disciplines[i].Children[j].Name)].Ell) + 25;
                        splines.Add(new CoordSpline(X1, Y1, Math.Abs(X1 + X2) / 2, Math.Abs(Y1 + Y2) / 2, X2, Y2, disciplines[i], disciplines[i].Children[j]));
                        Point[] _points = new Point[]
                           {
                                //start of line (and first segment)
                                new(splines[splines.Count - 1].X_Start, splines[splines.Count - 1].Y_Start),
                                //First control point:
                                new(splines[splines.Count - 1].X_Start, splines[splines.Count - 1].Y_Start + (splines[splines.Count - 1].Y_Middle - splines[splines.Count - 1].Y_Start) / 2),
                                //Second control point:
                                new(splines[splines.Count - 1].X_Middle - (splines[splines.Count - 1].X_Middle - splines[splines.Count - 1].X_Start) / 2, splines[splines.Count - 1].Y_Middle),
                                //end of first segment and start of second.
                                new(splines[splines.Count - 1].X_Middle, splines[splines.Count - 1].Y_Middle),
                                new(splines[splines.Count - 1].X_Middle + (splines[splines.Count - 1].X_End - splines[splines.Count - 1].X_Middle) / 2, splines[splines.Count - 1].Y_Middle),
                                new(splines[splines.Count - 1].X_End, splines[splines.Count - 1].Y_End - (splines[splines.Count - 1].Y_End - splines[splines.Count - 1].Y_Middle) / 2),
                                new(splines[splines.Count - 1].X_End, splines[splines.Count - 1].Y_End)
                            };

                        //Create the segment connectors
                        PathGeometry connectorGeometry = new()
                        {
                            Figures = new PathFigureCollection()
                            {
                                new PathFigure()
                                {
                                        //define the start of the smooth line
                                        StartPoint = _points[0],
                                        //define the coordinates of the smooth line
                                        Segments = new PathSegmentCollection()
                                            {
                                                new PolyBezierSegment(points: _points.Skip(1), isStroked: true)
                                            }
                                }
                            }
                        };

                        Path smoothCurve = new()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            Data = connectorGeometry
                        };

                        lines.Add(new CurvedLine(smoothCurve, disciplines[i], disciplines[i].Children[j]));
                    }
                }
            }

            DrawOrientedGraph();
        }

        private void DrawOrientedGraph()
        {
            if (stateOfGraph == false)
            {
                canvas.Children.Clear();
                for (int i = 0; i < lines.Count; i++)
                {
                    canvas.Children.Add(lines[i].Line);
                }

                for (int i = 0; i < ellipses.Count; i++)
                {
                    canvas.Children.Add(ellipses[i].Ell);
                    canvas.Children.Add(ellipses[i].Textbl);
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    System.Windows.Shapes.Rectangle ell = new System.Windows.Shapes.Rectangle();
                    ell.Width = 5;
                    ell.Height = 5;
                    SolidColorBrush bru = new SolidColorBrush();
                    bru.Color = Colors.Blue;
                    ell.Fill = bru;
                    double angle = Math.Atan(Math.Abs(splines[i].Y_Start - splines[i].Y_End) / Math.Abs(splines[i].X_Start - splines[i].X_End));
                    if (splines[i].X_Start - splines[i].X_End > 0 && splines[i].Y_Start - splines[i].Y_End > 0) //III quarter
                    {
                        Canvas.SetLeft(ell, splines[i].X_End + 25 * Math.Cos(angle) - 5);
                        Canvas.SetTop(ell, splines[i].Y_End + 25 * Math.Sin(angle) - 8);
                    }
                    else if (splines[i].X_Start - splines[i].X_End < 0 && splines[i].Y_Start - splines[i].Y_End > 0) // IV quarter
                    {
                        Canvas.SetLeft(ell, splines[i].X_End - 25 * Math.Cos(angle) + 3);
                        Canvas.SetTop(ell, splines[i].Y_End + 25 * Math.Sin(angle) - 2);
                    }
                    else if (splines[i].X_Start - splines[i].X_End > 0 && splines[i].Y_Start - splines[i].Y_End < 0) // II quarter
                    {
                        Canvas.SetLeft(ell, splines[i].X_End + 25 * Math.Cos(angle) - 10);
                        Canvas.SetTop(ell, splines[i].Y_End - 25 * Math.Sin(angle) - 5);
                    }
                    else if (splines[i].X_Start - splines[i].X_End < 0 && splines[i].Y_Start - splines[i].Y_End < 0) //I chapter
                    {
                        Canvas.SetLeft(ell, splines[i].X_End - 25 * Math.Cos(angle) + 5);
                        Canvas.SetTop(ell, splines[i].Y_End - 25 * Math.Sin(angle));
                    }
                    canvas.Children.Add(ell);
                }
                canvas.Width = FindMaxWidth();
                canvas.Height = FindMaxHeight();
            }
            else if (CheckTheRightnessOfSorting())
            {
                ShowData();
            }
            else
            {
                stateOfGraph = false;
                DrawOrientedGraph();
            }
        }

        private Boolean CheckTheRightnessOfSorting()
        {
            for (int i = 0; i < disciplines.Count; i++)
            {
                if (disciplines[i].Semester == 0)
                {
                    MessageBox.Show("Для дисциплины \"" + disciplines[i].Name + "\" не указан семестр!");
                    return false;
                }
            }

            for (int i = 0; i < disciplines.Count; i++)
            {
                for (int j = 0;  j < disciplines[i].Children.Count; j++)
                {
                    if (disciplines[i].Semester > disciplines[i].Children[j].Semester)
                    {
                        MessageBox.Show("Дисциплина " + disciplines[i].Name + "не может идти впереди дисциплины " + disciplines[i].Children[j].Name);
                        return false;
                    }
                }
            }

            return true;
        }

        private void DrawTitleOfSemesters()
        { 
            TextBlock text1 = new TextBlock();
            text1.Text = "1 СЕМЕСТР";
            Canvas.SetTop(text1, 20);
            Canvas.SetLeft(text1, 20);
            canvas.Children.Add(text1);
            TextBlock text2 = new TextBlock();
            text2.Text = "2 СЕМЕСТР";
            Canvas.SetTop(text2, 20);
            Canvas.SetLeft(text2, 225);
            canvas.Children.Add(text2);
            TextBlock text3 = new TextBlock();
            text3.Text = "3 СЕМЕСТР";
            Canvas.SetTop(text3, 20);
            Canvas.SetLeft(text3, 430);
            canvas.Children.Add(text3);
            TextBlock text4 = new TextBlock();
            text4.Text = "4 СЕМЕСТР";
            Canvas.SetTop(text4, 20);
            Canvas.SetLeft(text4, 635);
            canvas.Children.Add(text4);
            TextBlock text5 = new TextBlock();
            text5.Text = "5 СЕМЕСТР";
            Canvas.SetTop(text5, 20);
            Canvas.SetLeft(text5, 840);
            canvas.Children.Add(text5);
            TextBlock text6 = new TextBlock();
            text6.Text = "6 СЕМЕСТР";
            Canvas.SetTop(text6, 20);
            Canvas.SetLeft(text6, 1045);
            canvas.Children.Add(text6);
            TextBlock text7 = new TextBlock();
            text7.Text = "7 СЕМЕСТР";
            Canvas.SetTop(text7, 20);
            Canvas.SetLeft(text7, 1250);
            canvas.Children.Add(text7);
            TextBlock text8 = new TextBlock();
            text8.Text = "8 СЕМЕСТР";
            Canvas.SetTop(text8, 20);
            Canvas.SetLeft(text8, 1455);
            canvas.Children.Add(text8);

        }
        private void ShowData()
        {
            canvas.Children.Clear();
            DrawTitleOfSemesters();
            List<Discipline> list1 = new List<Discipline>();
            List<Discipline> list2 = new List<Discipline>();
            List<Discipline> list3 = new List<Discipline>();
            List<Discipline> list4 = new List<Discipline>();
            List<Discipline> list5 = new List<Discipline>();
            List<Discipline> list6 = new List<Discipline>();
            List<Discipline> list7 = new List<Discipline>();
            List<Discipline> list8 = new List<Discipline>();
            for (int i = 0; i < disciplines.Count(); i++)
            {
                switch (disciplines[i].Semester)
                {
                    case 1:
                        list1.Add(disciplines[i]);
                        break;
                    case 2:
                        list2.Add(disciplines[i]);
                        break;
                    case 3:
                        list3.Add(disciplines[i]);
                        break;
                    case 4:
                        list4.Add(disciplines[i]);
                        break;
                    case 5:
                        list5.Add(disciplines[i]);
                        break;
                    case 6:
                        list6.Add(disciplines[i]);
                        break;
                    case 7:
                        list7.Add(disciplines[i]);
                        break;
                    case 8:
                        list8.Add(disciplines[i]);
                        break;                        
                }
            }
            
            list1.Sort(
                delegate(Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list2.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list3.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list4.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list5.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list6.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list7.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });
            list8.Sort(
                delegate (Discipline p1, Discipline p2)
                {
                    return p1.Name.ToString().CompareTo(p2.Name.ToString());
                });

            if (list8.Count>0)                        //определение длины и ширины канваса
            {
                canvas.Width = 1650;
            }
            else if (list7.Count>0)
            {
                canvas.Width = 1430;
            }
            else if (list6.Count>0)
            {
                canvas.Width = 1230;
            }
            else if (list5.Count > 0)
            {
                canvas.Width = 1000;
            }
            else if (list4.Count>0)
            {
                canvas.Width = 810;
            }
            else if (list3.Count > 0)
            {
                canvas.Width = 600;
            }
            else if (list2.Count > 0)
            {
                canvas.Width = 400;
            }
            else if (list1.Count > 0)
            {
                canvas.Width = 250;
            }

            List<double> maxHeight = new List<double>();
            maxHeight.Add(list1.Count * 25 + 60);
            maxHeight.Add(list2.Count * 25 + 60);
            maxHeight.Add(list3.Count * 25 + 60);
            maxHeight.Add(list4.Count * 25 + 60);
            maxHeight.Add(list5.Count * 25 + 60);
            maxHeight.Add(list6.Count * 25 + 60);
            maxHeight.Add(list7.Count * 25 + 60);
            maxHeight.Add(list8.Count * 25 + 60);
            canvas.Height = maxHeight.Max<double>();

            for (int i = 0; i < list1.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list1[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 20);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list1[i].Children != null)
                {
                    for (int j = 0; j < list1[i].Children.Count; j++)
                    {
                        if (list2.Contains(list1[i].Children[j]))
                        { 
                            int index = list2.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 225, index * 25 + 70);
                        }

                        if (list3.Contains(list1[i].Children[j]))
                        {
                            int index = list3.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 430, index * 25 + 70);
                        }

                        if (list4.Contains(list1[i].Children[j]))
                        {
                            int index = list4.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 635, index * 25 + 70);
                        }

                        if (list5.Contains(list1[i].Children[j]))
                        {
                            int index = list5.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 840, index * 25 + 70);
                        }

                        if (list6.Contains(list1[i].Children[j]))
                        {
                            int index = list6.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 1045, index * 25 + 70);
                        }

                        if (list7.Contains(list1[i].Children[j]))
                        {
                            int index = list7.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list1[i].Children[j]))
                        {
                            int index = list8.IndexOf(list1[i].Children[j]);
                            DrawArrow(175.0, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list2.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list2[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 225);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list2[i].Children != null)
                {
                    for (int j = 0; j < list2[i].Children.Count; j++)
                    {
                        if (list3.Contains(list2[i].Children[j]))
                        {
                            int index = list3.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 430, index * 25 + 70);
                        }

                        if (list4.Contains(list2[i].Children[j]))
                        {
                            int index = list4.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 635, index * 25 + 70);
                        }

                        if (list5.Contains(list2[i].Children[j]))
                        {
                            int index = list5.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 840, index * 25 + 70);
                        }

                        if (list6.Contains(list2[i].Children[j]))
                        {
                            int index = list6.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 1045, index * 25 + 70);
                        }

                        if (list7.Contains(list2[i].Children[j]))
                        {
                            int index = list7.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list2[i].Children[j]))
                        {
                            int index = list8.IndexOf(list2[i].Children[j]);
                            DrawArrow(380, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list3.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list3[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 430);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list3[i].Children != null)
                {
                    for (int j = 0; j < list3[i].Children.Count; j++)
                    {
                        if (list4.Contains(list3[i].Children[j]))
                        {
                            int index = list4.IndexOf(list3[i].Children[j]);
                            DrawArrow(585, i * 25 + 70, 635, index * 25 + 70);
                        }

                        if (list5.Contains(list3[i].Children[j]))
                        {
                            int index = list5.IndexOf(list3[i].Children[j]);
                            DrawArrow(585, i * 25 + 70, 840, index * 25 + 70);
                        }

                        if (list6.Contains(list3[i].Children[j]))
                        {
                            int index = list6.IndexOf(list3[i].Children[j]);
                            DrawArrow(585, i * 25 + 70, 1045, index * 25 + 70);
                        }

                        if (list7.Contains(list3[i].Children[j]))
                        {
                            int index = list7.IndexOf(list3[i].Children[j]);
                            DrawArrow(585, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list3[i].Children[j]))
                        {
                            int index = list8.IndexOf(list3[i].Children[j]);
                            DrawArrow(585, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list4.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list4[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 635);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list4[i].Children != null)
                {
                    for (int j = 0; j < list4[i].Children.Count; j++)
                    {
                        if (list5.Contains(list4[i].Children[j]))
                        {
                            int index = list5.IndexOf(list4[i].Children[j]);
                            DrawArrow(790, i * 25 + 70, 840, index * 25 + 70);
                        }

                        if (list6.Contains(list4[i].Children[j]))
                        {
                            int index = list6.IndexOf(list4[i].Children[j]);
                            DrawArrow(790, i * 25 + 70, 1045, index * 25 + 70);
                        }

                        if (list7.Contains(list4[i].Children[j]))
                        {
                            int index = list7.IndexOf(list4[i].Children[j]);
                            DrawArrow(790, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list4[i].Children[j]))
                        {
                            int index = list8.IndexOf(list4[i].Children[j]);
                            DrawArrow(790, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list5.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list5[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 840);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list5[i].Children != null)
                {
                    for (int j = 0; j < list5[i].Children.Count; j++)
                    {

                        if (list6.Contains(list5[i].Children[j]))
                        {
                            int index = list6.IndexOf(list5[i].Children[j]);
                            DrawArrow(995, i * 25 + 70, 1045, index * 25 + 70);
                        }

                        if (list7.Contains(list5[i].Children[j]))
                        {
                            int index = list7.IndexOf(list5[i].Children[j]);
                            DrawArrow(995, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list5[i].Children[j]))
                        {
                            int index = list8.IndexOf(list5[i].Children[j]);
                            DrawArrow(995, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list6.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list6[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 1045);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list6[i].Children != null)
                {
                    for (int j = 0; j < list6[i].Children.Count; j++)
                    {

                        if (list7.Contains(list6[i].Children[j]))
                        {
                            int index = list7.IndexOf(list6[i].Children[j]);
                            DrawArrow(1200, i * 25 + 70, 1250, index * 25 + 70);
                        }

                        if (list8.Contains(list6[i].Children[j]))
                        {
                            int index = list8.IndexOf(list6[i].Children[j]);
                            DrawArrow(1200, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list7.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list7[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 1250);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
                if (list7[i].Children != null)
                {
                    for (int j = 0; j < list7[i].Children.Count; j++)
                    {
                        if (list8.Contains(list7[i].Children[j]))
                        {
                            int index = list8.IndexOf(list7[i].Children[j]);
                            DrawArrow(1405, i * 25 + 70, 1455, index * 25 + 70);
                        }
                    }
                }
            }

            for (int i = 0; i < list8.Count; i++)
            {
                Button butt = new Button();
                butt.Content = list8[i].Name;
                butt.Width = 155;
                butt.Height = 20;
                butt.Click += butt_Click;
                Canvas.SetLeft(butt, 1455);
                Canvas.SetTop(butt, i * 25 + 60);
                canvas.Children.Add(butt);
            }

        }

        private void butt_Click(Object sender, EventArgs e)
        {
            var senderBtn = sender as Button;
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Button)
                {
                    Button button= (Button)canvas.Children[i];
                    button.Background = Brushes.LightGray;
                    canvas.Children[i] = button;
                }
            }

            if (senderBtn != null)
            {
                String text = senderBtn.Content.ToString();
                if (text != null)
                {
                    TextBlock tb = new TextBlock();
                    Discipline disc = GetDisciplineByName(text);
                    tb.Text = disc.ToString();
                    TextSpace.Content = tb;
                    
                    for (int i = 0; i < canvas.Children.Count;i++)
                    {
                        if (canvas.Children[i] is Button)
                        { 
                            Button button = (Button)canvas.Children[i];
                            if (button.Content.ToString() == text)
                            {
                                button.Background = Brushes.White;
                                canvas.Children[i] = button;
                            }
                            else
                            {
                                for (int j = 0; j < disc.Children.Count; j++)
                                {
                                    if (button.Content.ToString() == disc.Children[j].Name)
                                    {
                                        button.Background = Brushes.Coral;
                                        canvas.Children[i] = button;
                                        break;
                                    }
                                }

                                for (int j = 0; j < disc.Ancestors.Count; j++)
                                {
                                    if (button.Content.ToString() == disc.Ancestors[j].Name)
                                    {
                                        button.Background = Brushes.PaleTurquoise;
                                        canvas.Children[i] = button;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        private void EllipseClickEvent(object sender, MouseButtonEventArgs e)
        {
            var senderEl = sender as Ellipse;
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    Ellipse ellipse = (Ellipse)canvas.Children[i];
                    int index = GetIndexOfEll(ellipse);
                    ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                    ellipses[index].Ell = ellipse;
                    canvas.Children[i] = ellipse;
                }
            }

            if (senderEl != null)
            {
                String text = ellipses[GetIndexOfEll(senderEl)].Textbl.Text.ToString();
                if (text != null)
                {
                    TextBlock tb = new TextBlock();
                    Discipline disc = GetDisciplineByName(text);
                    tb.Text = disc.ToString();
                    TextSpace.Content = tb;

                    for (int i = 0; i < canvas.Children.Count; i++)
                    {
                        if (canvas.Children[i] is Ellipse)
                        {
                            Ellipse ellipse = (Ellipse)canvas.Children[i];
                            int index = GetIndexOfEll(ellipse);
                            if (ellipses[index].Textbl.Text.ToString().Equals(text))
                            {
                                ellipse.Fill = new SolidColorBrush(Colors.DarkSeaGreen);
                                ellipses[index].Ell = ellipse;
                                canvas.Children[i] = ellipse;
                            }
                            else
                            {
                                
                                for (int j = 0; j < disc.Children.Count; j++)
                                {
                                    if (ellipses[index].Textbl.Text.ToString().Equals(disc.Children[j].Name))
                                    {
                                        ellipse.Fill = new SolidColorBrush(Colors.Coral);
                                        ellipses[index].Ell = ellipse;
                                        canvas.Children[i] = ellipse;
                                        break;
                                    }
                                }

                                for (int j = 0; j < disc.Ancestors.Count; j++)
                                {
                                    if (ellipses[index].Textbl.Text.ToString().Equals(disc.Ancestors[j].Name))
                                    {
                                        ellipse.Fill = new SolidColorBrush(Colors.PaleTurquoise);
                                        canvas.Children[i] = ellipse;
                                        ellipses[index].Ell = ellipse;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        private void EditClick(object sender, RoutedEventArgs e) // дописать редактирование
        {
            MenuItem mi = sender as MenuItem;
            Ellipse ellipse = null;
            if (mi != null)
            {
                ellipse = ((ContextMenu)mi.Parent).PlacementTarget as Ellipse;
                Discipline disc = GetDisciplineByName(ellipses[GetIndexOfEll(ellipse)].Textbl.Text);
                Request._name = disc.Name;
                Request._semester = disc.Semester;
                Request._comment = disc.Comment;
                //MessageBox.Show(disc.ToString());

                EditWindow editWindow = new();
                editWindow.ShowDialog();
                Discipline temp = new Discipline(Request._name, Request._semester, Request._comment);
                temp.Children = disc.Children;
                temp.Ancestors = disc.Ancestors;
                temp.BeingAdded = true;
                int index = GetIndexOf(disc);
                
                ellipses[GetIndexOfEll(ellipse)].Textbl.Text = Request._name;
                for (int i = 0; i < disciplines.Count; i++)
                {
                    for (int j = 0; j < disciplines[i].Ancestors.Count; j++)
                    {
                        if (disciplines[i].Ancestors[j].Equals(disc))
                        {
                            disciplines[i].Ancestors[j] = temp;
                        }
                    }

                    for (int j = 0; j < disciplines[i].Children.Count; j++)
                    {
                        if (disciplines[i].Children[j].Equals(disc))
                        {
                            disciplines[i].Children[j] = temp;
                        }
                    }
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartDiscipline.Equals(disc))
                    {
                        lines[i].StartDiscipline = temp;
                    }

                    if (lines[i].EndDiscipline.Equals(disc))
                    {
                        lines[i].EndDiscipline = temp;
                    }
                }

                for (int i = 0; i < splines.Count; i++)
                {
                    if (splines[i].StartDiscipline.Equals(disc))
                    {
                        splines[i].StartDiscipline = temp;
                    }

                    if (splines[i].EndDiscipline.Equals(disc))
                    {
                        splines[i].EndDiscipline = temp;
                    }
                }

                disciplines[index] = temp;
                Discipline1Relation.ItemsSource = GetNames(disciplines);
                Discipline2Relation.ItemsSource = GetNames(disciplines);
                DeletingDiscipline.ItemsSource = GetNames(disciplines);
                DrawOrientedGraph();

            }
            

        }

        private void DeleteClickContextMenu(object sender, RoutedEventArgs e) // сделать реализацию удаления
        {
            MenuItem mi = sender as MenuItem;
            Ellipse ellipse = null;
            if (mi != null)
            {
                ellipse = ((ContextMenu)mi.Parent).PlacementTarget as Ellipse;
                Discipline temp = GetDisciplineByName(ellipses[GetIndexOfEll(ellipse)].Textbl.Text);
                for (int i = 0; i < canvas.Children.Count; i++)
                {
                    if (canvas.Children[i] is Ellipse)
                    {
                        Ellipse ellipse1 = (Ellipse)canvas.Children[i];
                        ellipse1.Fill = new SolidColorBrush(Colors.LightGray);
                        canvas.Children[i] = ellipse1;
                    }
                }

                TextBlock tb = new TextBlock();
                tb.Text = "Область для вывода информации о дисциплине";
                TextSpace.Content = tb;
                string text = temp.Name;
                for (int i = 0; i < temp.Ancestors.Count; i++)
                {
                    disciplines[GetIndexOf(temp.Ancestors[i])].Children.Remove(temp);
                }

                for (int i = 0; i < temp.Children.Count; i++)
                {
                    disciplines[GetIndexOf(temp.Children[i])].Ancestors.Remove(temp);
                }

                
                Ellipse ellipse0 = ellipses[GetIndexOfEll(disciplines[GetIndexOf(temp)].Name)].Ell;
                int k = 0;
                if (lines.Count > 0)
                {
                    while (k == 0)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (lines[i].StartDiscipline == temp || lines[i].EndDiscipline == temp)
                            {
                                if (i == lines.Count - 1)
                                {
                                    lines.RemoveAt(i);
                                    splines.RemoveAt(i);
                                    k = 1;
                                }
                                else
                                {
                                    lines.RemoveAt(i);
                                    splines.RemoveAt(i);
                                    break;
                                }
                            }
                            else if (i == lines.Count - 1)
                            {
                                k = 1;
                            }
                        }
                    }
                }

                //MessageBox.Show("Done!");
                coords.RemoveAt(GetIndexOfCoord(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse)));
                ellipses.RemoveAt(GetIndexOfEll(disciplines[GetIndexOf(temp)].Name));
                disciplines.RemoveAt(GetIndexOf(temp));
                Discipline1Relation.ItemsSource = GetNames(disciplines);
                Discipline2Relation.ItemsSource = GetNames(disciplines);
                DeletingDiscipline.ItemsSource = GetNames(disciplines);
                DrawOrientedGraph();
                
            }
        }

        private void DrawArrow(double x1, double y1, double x2, double y2)
        {
            double d = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

            double X = x2 - x1;
            double Y = y2 - y1;

            double X3 = x2 - (X / d) * 5;
            double Y3 = y2 - (Y / d) * 5;

            double Xp = y2 - y1;
            double Yp = x1 - x2;

            double X4 = X3 + (Xp / d) * 5;
            double Y4 = Y3 + (Yp / d) * 5;
            double X5 = X3 - (Xp / d) * 5;
            double Y5 = Y3 - (Yp / d) * 5;

            Line line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };
            canvas.Children.Add(line);

            line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                X1 = x2,
                Y1 = y2,
                X2 = X4,
                Y2 = Y4
            };
            canvas.Children.Add(line);

            line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                X1 = x2,
                Y1 = y2,
                X2 = X5,
                Y2 = Y5
            };
            canvas.Children.Add(line);
        }


    }
    
}
