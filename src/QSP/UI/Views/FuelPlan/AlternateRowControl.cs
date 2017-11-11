﻿using QSP.UI.Presenters.FuelPlan;
using QSP.UI.Util;
using QSP.UI.Views.Route;
using QSP.UI.Views.Route.Actions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Routes = QSP.RouteFinding.Routes;

namespace QSP.UI.Views.FuelPlan
{
    public partial class AlternateRowControl : UserControl, IAlternateRowView
    {
        private AlternateRowPresenter presenter;
        private ActionContextMenu actionContextMenuView;
        private Form parentForm;

        public string ICAO { set => IcaoTxtBox.Text = value; }
        public IEnumerable<string> RunwayList { set => RwyComboBox.SetItems(value); }
        public string DistanceInfo { set => DisLbl.Text = value; }

        public string Route
        {
            get => RouteTxtBox.Text;
            set => RouteTxtBox.Text = value;
        }

        public void ShowInfo(string info) => parentForm.ShowInfo(info);
        public void ShowWarning(string warning) => parentForm.ShowWarning(warning);

        // TODO: Why not use default ParentForm?
        public void ShowMap(Routes.Route route)
        {
            ShowMapHelper.ShowMap(route, parentForm.Size, parentForm);
        }

        public void ShowMapBrowser(Routes.Route route)
        {
            ShowMapHelper.ShowMap(route, parentForm.Size, parentForm, true, true);
        }

        public AlternateRowControl()
        {
            InitializeComponent();
        }

        public void Init(AlternateRowPresenter presenter, Form parentForm)
        {
            actionContextMenuView = new ActionContextMenu();

            this.presenter = presenter;
            this.parentForm = parentForm;
        }

        private void SubscribeActionMenuEvents()
        {
            var a = actionContextMenuView;
            a.FindToolStripMenuItem.Click += (s, e) => presenter.FindRoute();
            a.AnalyzeToolStripMenuItem.Click += (s, e) => presenter.AnalyzeRoute();
            a.ExportToolStripMenuItem.Click += (s, e) => presenter.ExportRouteFiles();
            a.MapToolStripMenuItem.Click += (s, e) => presenter.ShowMap();
            a.MapInBrowserToolStripMenuItem.Click += (s, e) => presenter.ShowMapBrowser();
        }

        private void FindBtnClick(object sender, EventArgs e)
        {
            using (var frm = new FindAltnForm())
            {
                frm.AlternateSet += (_s, _e) => IcaoTxtBox.Text = frm.SelectedIcao;

                var altnPresenter = presenter.FindAltnPresenter(frm);
                frm.Init(presenter.DestIcao, altnPresenter);
                frm.ShowDialog();
            }
        }
    }
}
