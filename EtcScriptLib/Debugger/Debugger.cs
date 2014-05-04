using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EtcScriptLib.Debugger
{
	public partial class Debugger : Form
	{
		VirtualMachine.ExecutionContext Context;

		public Debugger()
		{
			InitializeComponent();
		}

		public Debugger(VirtualMachine.ExecutionContext Context)
		{
			InitializeComponent();
			SetContext(Context);
		}

		public void SetContext(VirtualMachine.ExecutionContext Context)
		{
			this.Context = Context;
			this.disassembledView.Context = Context;
			this.stackView.Context = Context;
			this.stackView.Scrollbar = this.stackScrollBar;
			this.registerView1.Context = Context;
			this.Invalidate(true);

			if (Context.ExecutionState == VirtualMachine.ExecutionState.Blocked)
			{
				this.stepButton.Enabled = true;
				this.button1.Enabled = true;
			}
		}

		private void stepButton_Click(object sender, EventArgs e)
		{
			if (Context.ExecutionState == VirtualMachine.ExecutionState.Blocked)
			{
				Context.ExecutionState = VirtualMachine.ExecutionState.Running;
				VirtualMachine.VirtualMachine.ExecuteSingleInstruction(Context);
				if (Context.ExecutionState == VirtualMachine.ExecutionState.Running)
					Context.ExecutionState = VirtualMachine.ExecutionState.Blocked;
			}

			this.stackScrollBar.Maximum = Context.Stack.Count;

			if (Context.ExecutionState != VirtualMachine.ExecutionState.Blocked)
			{
				this.stepButton.Enabled = false;
				this.button1.Enabled = false;
			}

			this.Invalidate(true);
		}

		private void stackScrollBar_ValueChanged(object sender, EventArgs e)
		{
			stackView.Invalidate();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (Context.ExecutionState == VirtualMachine.ExecutionState.Blocked)
			{
				Context.ExecutionState = VirtualMachine.ExecutionState.Running;
				VirtualMachine.VirtualMachine.ExecuteUntilFinished(Context);
			}

			this.stackScrollBar.Maximum = Context.Stack.Count;

			if (Context.ExecutionState != VirtualMachine.ExecutionState.Blocked)
			{
				this.stepButton.Enabled = false;
				this.button1.Enabled = false;
			}

			this.Invalidate(true);
		}
	}
}
