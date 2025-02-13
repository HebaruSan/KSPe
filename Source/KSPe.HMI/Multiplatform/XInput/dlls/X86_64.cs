﻿/*
	This file is part of KSPe, a component for KSP API Extensions/L
		© 2018-22 LisiasT : http://lisias.net <support@lisias.net>

	KSPe API Extensions/L is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	KSPe API Extensions/L is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with KSPe API Extensions/L. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with KSPe API Extensions/L. If not, see <https://www.gnu.org/licenses/>.

*/
using System.Runtime.InteropServices;
using GamePadState = KSPe.HMI.Multiplatform.XInput.GamePad.State;

namespace KSPe.HMI.Multiplatform.XInput.dlls
{
	internal class X86_64 : Ifc
	{
		internal X86_64()
		{
			Util.SystemTools.Assembly.LoadAndStartup("InputInterface.x86_64"); // TODO: FIX ME! How to load native DLLs?
		}

		uint Ifc.XInputGamePadGetState(uint playerIndex, out GamePadState.RawState state)
		{
			return dll_XInputGamePadGetState(playerIndex, out state);
		}

		void Ifc.XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor)
		{
			dll_XInputGamePadSetState(playerIndex, leftMotor, rightMotor);
		}

		[DllImport("XInputInterface.x86_64", EntryPoint = "XInputGamePadGetState")]
		private static extern uint dll_XInputGamePadGetState(uint playerIndex, out GamePadState.RawState state);

		[DllImport("XInputInterface.x86_x64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "XInputGamePadSetState")]
		private static extern void dll_XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor);
	}
}
