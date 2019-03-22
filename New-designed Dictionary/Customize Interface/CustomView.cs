using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace New_designed_Dictionary.Customize_Interface
{
    public static class CustomView
    {
        static public childItem FindVisualChild<childItem>(DependencyObject obj)
        where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        static public List<ListBoxItem> lvSourceItems(ListBox lb)
        {
            List<ListBoxItem> listViewItems = new List<ListBoxItem>();
            for (int i = 0; i < lb.Items.Count; i++)
            {
                listViewItems.Add((ListBoxItem)lb.ItemContainerGenerator.ContainerFromIndex(i));
                int f = (lb.Items.IndexOf(lb.Items[i].ToString()));
            }
            return listViewItems;
        } // ListBox
        static public List<ListBoxItem> lvSourceItems(ListView lv)
        {
            List<ListBoxItem> listViewItems = new List<ListBoxItem>();
            for (int i = 0; i < lv.Items.Count; i++)
            {
                listViewItems.Add((ListBoxItem)lv.ItemContainerGenerator.ContainerFromIndex(i));
                int f = (lv.Items.IndexOf(lv.Items[i].ToString()));
            }
            return listViewItems;
        } // ListView

        static public void ChangeCheckVisibility(ListBox lb, string iconName)
        {
            try
            {
                for (int i = 0; i < lvSourceItems(lb).Count; i++)
                {
                    if (i == lb.SelectedIndex)
                    {
                        GetPackIcon(i, lb, iconName).Visibility = Visibility.Visible;
                    }
                    if (i != lb.SelectedIndex)
                    {
                        GetPackIcon(i, lb, iconName).Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception e) { string ex = e.Message; }
        } // ListBox
        static public void ChangeCheckVisibility(ListView lv, string iconName)
        {
            try
            {
                for (int i = 0; i < lvSourceItems(lv).Count; i++)
                {
                    if (i == lv.SelectedIndex)
                    {
                        GetPackIcon(i, lv, iconName).Visibility = Visibility.Visible;
                    }
                    if (i != lv.SelectedIndex)
                    {
                        GetPackIcon(i, lv, iconName).Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception e) { string ex = e.Message; }
        } // ListView
        

        static public PackIcon GetPackIcon(int i, ListView lv, string iconName)
        {
            ListBoxItem myListBoxItem = (ListBoxItem)(lv.ItemContainerGenerator.ContainerFromIndex(i));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            PackIcon target = (PackIcon)myDataTemplate.FindName(iconName, myContentPresenter);
            return target;
        } // ListView
        static public PackIcon GetPackIcon(int i, ListBox lb, string iconName)
        {
            ListBoxItem myListBoxItem = (ListBoxItem)(lb.ItemContainerGenerator.ContainerFromIndex(i));
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            PackIcon target = (PackIcon)myDataTemplate.FindName(iconName, myContentPresenter);
            return target;
        } // ListBox
    }
}
