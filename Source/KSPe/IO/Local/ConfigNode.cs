﻿/*
	This file is part of KSPe, a component for KSP API Extensions/L
	(C) 2018-19 Lisias T : http://lisias.net <support@lisias.net>

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

namespace KSPe.IO.Local
{
	public class ConfigNode: WritableConfigNode
	{
		protected ConfigNode(string name, string fn): base(name)
		{
			this.Path = fn;
		}

		public new ConfigNode Load()
		{
			return (ConfigNode)base.Load();
		}

		public static ConfigNode ForType<T>(string name = null)
		{
			string path = IO.File<T>.Local.FullPathName(true, (name ?? typeof(T).FullName) + ".cfg");
			return new ConfigNode(name, path);
		}

		public static ConfigNode ForType<T>(string name, string filename)
		{
			string path = IO.File<T>.Local.FullPathName(true, filename);
			return new ConfigNode(name, path);
		}

		public static ConfigNode ForType<T>(string name, string fn, params string[] fns)
		{
			string path = IO.File<T>.Local.FullPathName(true, fn, fns);
			return new ConfigNode(name, path);
		}

		public static string[] ListForType<T>(string mask = "*.cfg", bool subdirs = false)
		{
			string dir = IO.File<T>.Local.FullPathName(false, ".");
			string[] files = ReadableConfigNode.ListFiles(dir, mask, subdirs);
			return files;
		}
	}
}
