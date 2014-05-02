﻿//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using OpenCover.UI.Views;
using OpenCover.UI.Model.Test;

/// <summary>
/// Tests Explorer Toolbar Commands
/// </summary>
namespace OpenCover.UI.Commands
{
	internal class TestsExplorerToolbarCommands
	{
		private static bool _initialized;
		private static OleMenuCommand _currentSelected;
		private static List<OleMenuCommand> _allCommands;

		/// <summary>
		/// Static constructor for TestsExplorerToolbarCommands class.
		/// </summary>
		static TestsExplorerToolbarCommands()
		{
			if (!_initialized)
			{
				Initialize();
			}

			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionOpened += () => EnableDisableCommands(true);
			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionClosing += () => EnableDisableCommands(false);
		}

		/// <summary>
		/// Gets all the commands.
		/// </summary>
		/// <value>
		/// The commands.
		/// </value>
		internal static IEnumerable<OleMenuCommand> Commands
		{
			get
			{
				return _allCommands;
			}
		}

		internal static TestMethodGroupingField CurrentSelectedGroupBy
		{
			get;
			private set;
		}

		/// <summary>
		/// Enables or disables commands.
		/// </summary>
		/// <param name="state">Sets Enabled property of all commands to 'state'</param>
		private static void EnableDisableCommands(bool state)
		{
			_allCommands.ForEach(c => c.Enabled = state);
		}

		internal static void UpdateSelectedGroupBy(TestMethodGroupingField field)
		{
			CurrentSelectedGroupBy = field;
			UpdateCheckedMethods(GetCommand());
		}

		/// <summary>
		/// Initializes all test explorer toolbar commands.
		/// </summary>
		private static void Initialize()
		{
			if (!_initialized)
			{
				_allCommands = new List<OleMenuCommand>();

				_currentSelected = GetCommand((int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton);
				_currentSelected.Checked = true;
				_allCommands.Add(_currentSelected);

				_allCommands.Add(GetCommand((int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByTraitButton));

				_allCommands.Add(GetCommand((int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByProjectButton));

				_allCommands.Add(GetCommand((int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByOutcomeButton));

				// Refresh command
				_allCommands.Add(new OleMenuCommand((s, e) => OpenCoverUIPackage.Instance.VSEventsHandler.BuildSolution(),
									new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
													(int)PkgCmdIDList.OpenCoverTestExplorerToolbarRefreshButton)) { Enabled = false });

				_initialized = true;
			}
		}

		private static OleMenuCommand GetCommand(int buttonId)
		{
			return new OleMenuCommand(Executed, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet, buttonId)) { Enabled = false };
		}

		/// <summary>
		/// Event handler for Group By Command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private static void Executed(object sender, EventArgs e)
		{
			var clicked = sender as OleMenuCommand;

			UpdateCheckedMethods(clicked);

			// Group tests based on the clicked command
			switch (clicked.CommandID.ID)
			{
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton:
					CurrentSelectedGroupBy = TestMethodGroupingField.Class;
					break;
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByTraitButton:
					CurrentSelectedGroupBy = TestMethodGroupingField.Trait;
					break;
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByProjectButton:
					CurrentSelectedGroupBy = TestMethodGroupingField.Project;
					break;
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByOutcomeButton:
					CurrentSelectedGroupBy = TestMethodGroupingField.Outcome;
					break;
			}

			OpenCoverUIPackage.Instance.ToolWindows.OfType<TestExplorerToolWindow>().First().TestExplorerControl.ChangeGroupBy(CurrentSelectedGroupBy);
		}

		/// <summary>
		/// Updates the selected methods.
		/// </summary>
		/// <param name="clickedCommand">The clicked command.</param>
		private static void UpdateCheckedMethods(OleMenuCommand clickedCommand)
		{
			if (_currentSelected != null)
			{
				_currentSelected.Checked = false;
			}

			_currentSelected = clickedCommand;

			_currentSelected.Checked = true;
		}

		private static OleMenuCommand GetCommand()
		{
			int commandID = 0;

			switch (CurrentSelectedGroupBy)
			{
				case TestMethodGroupingField.Class:
					commandID = PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton;
					break;
				case TestMethodGroupingField.Trait:
					commandID = PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByTraitButton;
					break;
				case TestMethodGroupingField.Project:
					commandID = PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByProjectButton;
					break;
				case TestMethodGroupingField.Outcome:
					commandID = PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByOutcomeButton;
					break;
				default:
					commandID = PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton;
					break;
			}

			return Commands.FirstOrDefault(c => c.CommandID.ID == commandID);
		}
	}
}