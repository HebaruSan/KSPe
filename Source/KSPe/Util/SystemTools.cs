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
namespace KSPe.Util
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using SType = System.Type;

	using SIO = System.IO;
	using SReflection = System.Reflection;


	public static class SystemTools
	{
		public static class Interface
		{
			public static object CreateInstanceByInterface(SType iifc)
			{
				Log.debug("Looking for {0}", iifc.FullName);
				foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
					foreach (SType type in assembly.GetTypes())
						foreach (SType ifc in type.GetInterfaces())
						{
							Log.debug("Checking {0} {1} {2}", assembly, type, ifc);
							/*
							 * This caught me with my pants down!
							 * (typeof(Interface).Equals(ifc.GetType())) and (typeof(Interface) == ifc.GetType()) does not work!
							 */
							if (iifc.ToString() == ifc.ToString()) // Don't ask. This works...
							{
								Log.debug("Found one! {0}", ifc);
								return CreateInstance(type);
							}
						}
				return null;
			}

			public static object CreateInstanceByInterfaceName(string ifcName)
			{
				Log.debug("Looking for {0}", ifcName);
				foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
					foreach (SType type in assembly.GetTypes())
						foreach (SType ifc in type.GetInterfaces())
						{
							Log.debug("Checking {0} {1} {2}", assembly, type, ifc);
							if (ifcName == ifc.ToString())
							{
								Log.debug("Found one! {0}", ifc);
								return CreateInstance(type);
							}
						}
				return null;
			}

			internal static object CreateInstance(System.Type type)
			{
				object r = System.Activator.CreateInstance(type);
				Log.debug("Type of result {0}", r.GetType());
				return r;
			}

			private static readonly KSPe.Util.Log.Logger Log = KSPe.Util.Log.Logger.CreateForType<Startup>("KSPe.SystemTools", "Type", 0);
		}

		public static class Type
		{
			public static class Finder
			{
				private static readonly Dictionary<string, SType> TYPES = new Dictionary<string, SType>();

				public static bool ExistsBy(string ns, string name) => ExistsByQualifiedName(ns + "." + name);
				public static bool ExistsByQualifiedName(string qn)
				{
					lock (TYPES)
					{
						if (TYPES.ContainsKey(qn)) return true;
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
							foreach (SType type in assembly.GetTypes()) if (qn.Equals(string.Format("{0}.{1}", type.Namespace, type.Name)))
							{
								TYPES.Add(qn, type);
								return true;
							}
					}
					return false;
				}

				public static SType FindBy(string ns, string name) => FindByQualifiedName(ns + "." + name);
				public static SType FindByQualifiedName(string qn)
				{
					lock(TYPES)
					{ 
						if (TYPES.ContainsKey(qn)) return TYPES[qn];
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
							foreach (SType type in assembly.GetTypes()) if (qn.Equals(string.Format("{0}.{1}", type.Namespace, type.Name)))
							{
								TYPES.Add(qn, type);
								return type;
							}
					}
					throw new DllNotFoundException("An Add'On Support DLL was not loaded. Missing type : " + qn);
				}

				public static SType FindByInterface(string ns, string name) => FindByInterfaceName(ns + "." + name);
				public static SType FindByInterfaceName(string qn)
				{
					lock(TYPES)
					{ 
						if (TYPES.ContainsKey(qn)) return TYPES[qn];
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
							foreach (SType type in assembly.GetTypes())
								foreach (SType ifc in type.GetInterfaces()) if (qn.Equals(string.Format("{0}.{1}", ifc.Namespace, ifc.Name)))
								{
									TYPES.Add(qn, type);
									return type;
								}
					}
					throw new DllNotFoundException("An Add'On Support DLL was not loaded. Missing Interface : " + qn);
				}

				public static SType FindBy(SType ifc)
				{
					lock(TYPES)
					{ 
						if (TYPES.ContainsKey(ifc.FullName)) return TYPES[ifc.ToString()];
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
							foreach (SType type in assembly.GetTypes())
								foreach (SType i in type.GetInterfaces()) if (i.Equals(ifc))
								{
									TYPES.Add(ifc.ToString(), type);
									return type;
								}
					}
					throw new DllNotFoundException("An Add'On Support DLL was not loaded. Missing Interface : " + ifc.FullName);
				}
			}

			public static class Search
			{
				public static IEnumerable<SType> ByInterfaceName(string qn)
				{
					foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
						foreach (SType type in assembly.GetTypes())
							foreach (SType ifc in type.GetInterfaces()) if (qn.Equals(string.Format("{0}.{1}", ifc.Namespace, ifc.Name)))
								yield return type;
				}

				public static IEnumerable<SType> By(SType ifc)
				{
					foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
						foreach (SType type in assembly.GetTypes())
							foreach (SType i in type.GetInterfaces()) if (i.Equals(ifc))
								yield return type;
				}
			}
		}

		// TODO: Remove this on Version 2.5
		[Obsolete("SystemTools.TypeFinder is obsolete. Please use Type.Finder instead.")]
		public static class TypeFinder
		{
			public static bool ExistsByQualifiedName(string qn) => Type.Finder.ExistsByQualifiedName(qn);
			public static SType FindByQualifiedName(string qn) => Type.Finder.FindByQualifiedName(qn);
			public static SType FindByInterfaceName(string qn) => Type.Finder.FindByInterfaceName(qn);
			public static SType FindByInterface(SType ifc) => Type.Finder.FindBy(ifc);
		}

		// TODO: Remove this on Version 2.5
		[Obsolete("SystemTools.TypeSearch is obsolete. Please use Type.Search instead.")]
		public static class TypeSearch
		{
			public static IEnumerable<SType> ByInterfaceName(string qn) => Type.Search.ByInterfaceName(qn);
			public static IEnumerable<SType> ByInterface(SType ifc) => Type.Search.By(ifc);
		}

		public static class Assembly
		{
			public static class Finder
			{
				private static readonly Dictionary<string, SReflection.Assembly> ASSEMBLIES = new Dictionary<string, SReflection.Assembly>();
				public static bool ExistsByName(string qn)
				{
					lock (ASSEMBLIES)
					{
						if (ASSEMBLIES.ContainsKey(qn)) return true;
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) if (assembly.GetName().Name.Equals(qn))
						{
							ASSEMBLIES.Add(qn, assembly);
							return true;
						}
					}
					return false;
				}
				public static SReflection.Assembly FindByName(string qn)
				{
					lock(ASSEMBLIES)
					{ 
						if (ASSEMBLIES.ContainsKey(qn)) return ASSEMBLIES[qn];
						foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) if (assembly.GetName().Name.Equals(qn))
						{
							ASSEMBLIES.Add(qn, assembly);
							return assembly;
						}
					}
					throw new DllNotFoundException("An Add'On Support DLL was not loaded. Missing Assembly : " + qn);
				}
			}

			public static class Search
			{
				public static IEnumerable<SReflection.Assembly> ByName(string name)
				{
					foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
						if (assembly.GetName().Name.Equals(name))
							yield return assembly;
				}
			}

			public class Loader : IDisposable
			{
				protected static readonly object MUTEX = new object();
				protected readonly string @namespace;
				protected readonly string effectivePath;
				protected readonly string searchPath;

				internal Loader() { this.@namespace = null; }
				internal Loader(string @namespace, string effectivePath, params string[] subdirs)
				{
					this.@namespace = @namespace;
					this.effectivePath = effectivePath;
					this.searchPath = this.buildSearchPath(subdirs);
					LOG.debug("Assembly searchPath: {0}", this.searchPath);
					this.EnterCritical();
				}
				// TODO: Change this on Version 2.5
				[Obsolete("Assembly.Loader(string) will be made internal on Release 2.5")]
				public Loader(string @namespace, params string[] subdirs) : this(@namespace, @namespace, subdirs) { }

				protected string buildSearchPath(params string[] subdirs)
				{
					List<string> parms = new List<string>(subdirs);
					parms.Insert(0, "PluginData");	// FIXME: Rework this on 2.5. The internal loader should be able to load DLLs from anyplace
					string[] sd = parms.ToArray();
					return this.TryPath("Plugins", sd)
										?? this.TryPath("Plugin", sd)
										?? this.TryPath(".", sd)
										?? throw new DllNotFoundException(
											string.Format("{0}'s DLL search path does not exists!", this.@namespace)
										)
							;
				}

				protected virtual string TryPath(string path, params string[] subdirs)
				{
					string t = SIO.Path.Combine(this.@namespace, path);
					LOG.debug("Assembly TryPath: {0}", t);
					LOG.force("Assembly TryPath: {0}", t);
					string p = IO.Hierarchy.GAMEDATA.SolveFull(false, t, subdirs);
					if (IO.Directory.Exists(p))
						return IO.Hierarchy.GAMEDATA.Solve(false, t, subdirs);

					return null;
				}

				protected void EnterCritical()
				{
					Monitor.Enter(MUTEX);
					Assembly.AddSearchPath(this.searchPath);

				}

				void IDisposable.Dispose()
				{
					Assembly.RemoveSearchPath(this.searchPath);
					Monitor.Exit(MUTEX);
				}

				public SReflection.Assembly LoadAndStartup(string assemblyName)
				{
					return Assembly.LoadAndStartup(assemblyName);
				}
			}

			public class Loader<T> : Loader
			{
				private readonly SType type;

				public Loader(params string[] subdirs) : base(
											SystemTools.Reflection.Version<T>.Namespace,
											SystemTools.Reflection.Version<T>.EffectivePath,
											subdirs)
				{
					this.type = typeof(T);
				}

				protected override string TryPath(string path, params string[] subdirs)
				{
					string p = IO.Hierarchy<T>.GAMEDATA.SolveFull(false, path, subdirs);
					LOG.debug("Assembly TryPath: {0}", p);
					if (IO.Directory.Exists(p))
						return IO.Hierarchy<T>.GAMEDATA.Solve(false, path, subdirs);
					return null;
				}
			}

			// Obrigatory reading:
			//	https://flylib.com/books/en/4.331.1.56/1/
			//	https://weblog.west-wind.com/posts/2016/dec/12/loading-net-assemblies-out-of-seperate-folders
			//	https://docs.microsoft.com/en-us/archive/blogs/suzcook/loadfile-vs-loadfrom
			//	https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.load?view=netcore-3.1
			//	https://jeremylindsayni.wordpress.com/2019/02/11/instantiating-a-c-object-from-a-string-using-activator-createinstance-in-net/
			// Solution used:
			//	from https://weblog.west-wind.com/posts/2016/dec/12/loading-net-assemblies-out-of-seperate-folders
			//	see also https://docs.microsoft.com/en-us/dotnet/standard/assembly/resolve-loads?redirectedfrom=MSDN

			[Obsolete("Assembly.AddSearchPath(string) will be made internal on Release 2.5")]
			public static void AddSearchPath(string path)
			{
				string fullpath = IO.Hierarchy.ROOT.SolveFull(false, path);

				if (!IO.Directory.Exists(fullpath))
					throw new SIO.FileNotFoundException(string.Format("The path {0} doesn't resolve to a valid DLL search path!", path));

				LOG.debug("AddSearchPath {0}", path);
				lock(CUSTOM_SEARCH_PATHS)
					if (!CUSTOM_SEARCH_PATHS.Contains(path))
						CUSTOM_SEARCH_PATHS.Add(path);
				LOG.debug("CUSTOM_SEARCH_PATHS : {0}", string.Join(":", CUSTOM_SEARCH_PATHS.ToArray()));
			}

			[Obsolete("Assembly.RemoveSearchPath(string) will be made internal on Release 2.5")]
			public static void RemoveSearchPath(string path)
			{
				LOG.debug("RemoveSearchPath {0}", path);
				lock(CUSTOM_SEARCH_PATHS)
					if (CUSTOM_SEARCH_PATHS.Contains(path))
						CUSTOM_SEARCH_PATHS.Remove(path);
				LOG.debug("CUSTOM_SEARCH_PATHS : {0}", string.Join(":", CUSTOM_SEARCH_PATHS.ToArray()));
			}

			[Obsolete("Assembly.LoadAndStartup(string) will be made internal on Release 2.5")]
			public static SReflection.Assembly LoadAndStartup(string assemblyName)
			{
				SReflection.Assembly assembly = System.AppDomain.CurrentDomain.Load(assemblyName);
				foreach (SType type in assembly.GetTypes())
				{
					if ("Startup" != type.Name) continue;

					object instance = System.Activator.CreateInstance(type);
					InvokeOrNull(type, instance, "Awake");
					InvokeOrNull(type, instance, "Start");
					break;
				}
				return assembly;
			}

			// These ones don't load the Assembly on the same context from the caller.
			// DAMN. I will keep them however, these ones can be useful somehow.

			[Obsolete("This call doesn't loads the Assembly on the same context as the caller. Unexpected cast problems (among others) can happen. Consider using KSPe.Util.SystemTools.Assembly.AddSearchPath(path) to register a folder and using System.AppDomain.CurrentDomain.Load(name) instead.")]
			public static SReflection.Assembly LoadFromFile(string pathname)
			{
				byte[] rawAssembly;
				using (SIO.FileStream fs = new SIO.FileStream(pathname, SIO.FileMode.Open))
				{
					rawAssembly = new byte[(int)fs.Length];
					fs.Read(rawAssembly, 0, rawAssembly.Length);
				}
				return System.AppDomain.CurrentDomain.Load(rawAssembly);
				//return Reflection.Assembly.LoadFrom(pathname);
			}

			[Obsolete("This call doesn't loads the Assembly on the same context as the caller. Unexpected cast problems (among others) can happen. Consider using KSPe.Util.SystemTools.Assembly.AddSearchPath(path) to register a folder and using KSPe.Util.SystemTools.Assembly.LoadAndStartup(name) instead.")]
			public static SReflection.Assembly LoadFromFileAndStartup(string pathname)
			{
				SReflection.Assembly assembly = LoadFromFile(pathname);
				foreach (SType type in assembly.GetTypes())
				{
					if ("Startup" != type.Name) continue;

					object instance = System.Activator.CreateInstance(type);
					InvokeOrNull(type, instance, "Awake");
					InvokeOrNull(type, instance, "Start");
					break;
				}
				return assembly;
			}

			private static object InvokeOrNull(SType t, object o, string methodName)
			{
				SReflection.MethodInfo method = t.GetMethod(methodName, SReflection.BindingFlags.Public | SReflection.BindingFlags.Instance);
				method = method ?? t.GetMethod(methodName, SReflection.BindingFlags.NonPublic | SReflection.BindingFlags.Instance);
				if (null != method)	return method.Invoke(o, null);
				return null;
			}

			private static readonly System.Collections.Generic.List<string> CUSTOM_SEARCH_PATHS = new System.Collections.Generic.List<string>();
			private static SReflection.Assembly AssemblyResolve(object sender, System.ResolveEventArgs args)
			{
				// Ignore missing resources
				if (args.Name.Contains(".resources")) return null;

				// check for assemblies already loaded
				foreach (SReflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
					if (assembly.GetName().Name == args.Name)
					{	// We had found it. Let's check for a conflict.
						string asmFile = FindThisGuy(args.Name, false);
						if (null != asmFile && assembly.Location != asmFile)
							LOG.error("Found a duplicated Assembly for {0} on file {1}. This is an error, there can be only one! #highlanderFeelings", args.Name, asmFile);
						return assembly;
					}

				{
					string asmFile = FindThisGuy(args.Name, true);
					if (null != asmFile) try
					{
						LOG.force("Found it on {0}.", asmFile);
						//return LoadAssemblyByKsp(args.Name, asmFile);
						return SReflection.Assembly.LoadFrom(asmFile);
					}
					catch (System.Exception ex)
					{
						LOG.error("Error {0} loading {1} from {2}!", ex.Message, args.Name, asmFile);
						return null;
					}
				}

				return null;
			}

			private static string FindThisGuy(string assemblyName, bool verbose)
			{
				// Try to load by filename - split out the filename of the full assembly name
				// and append the base path of the original assembly (ie. look in the same dir)
				string filename = assemblyName.Split(',')[0] + ".dll";

				lock(CUSTOM_SEARCH_PATHS)
					foreach (string path in CUSTOM_SEARCH_PATHS)
					{
						if (verbose) LOG.force("Looking for {0} on {1}...", filename, path);
						string asmFile = IO.Path.Combine(path,filename);
						if (SIO.File.Exists(asmFile)) return asmFile;
					}
				return null;
			}

			static Assembly()
			{
				System.AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
				LOG.force("Hooked.");
			}

			internal static readonly KSPe.Util.Log.Logger LOG = KSPe.Util.Log.Logger.CreateForType<KSPe.Startup>("KSPe", "Binder", 0);
		}

		public static class Reflection
		{
			internal static SType GetClass(SType klass, string innerClassName) => klass?.GetNestedType(innerClassName);
			internal static Y GetField<Y>(SType klass, string fieldName, Y defaultValue)
			{
				Y r = defaultValue;
				if (null != klass) try
				{
					r = (Y)klass.GetField(fieldName).GetValue(null);
				}
				catch (ArgumentNullException) { }
				catch (NotSupportedException) { }
				catch (FieldAccessException) { }
				catch (ArgumentException) { }
				catch (NullReferenceException) { }
				return r;
			}

			internal static class Version
			{
				public static string Namespace(SType klass) => GetField<string>(klass, "Namespace", klass.Namespace);
				public static string Vendor(SType klass) => GetField<string>(klass, "Vendor", null);
				public static string FriendlyName(SType klass) => GetField<string>(klass, "FriendlyName", klass.Name);

				public static int major(SType klass) => GetField<int>(klass, "major", 0);
				public static int minor(SType klass) => GetField<int>(klass, "minor", 0);
				public static int patch(SType klass) => GetField<int>(klass, "patch", 0);
				public static int build(SType klass) => GetField<int>(klass, "build", 0);
				public static System.Version V(SType klass) => new System.Version(major(klass), minor(klass), patch(klass), build(klass));
				public static string Number(SType klass) => GetField<string>(klass, "Number", "0.0.0.0");
				public static string Text(SType klass) => GetField<string>(klass, "Text", "0.0.0.0");
				public static string EffectivePath(SType klass) => null == Vendor(klass) ? Namespace(klass) : SIO.Path.Combine(Vendor(klass), Namespace(klass));
			}

			public static class Version<T>
			{
				public static SType Class = Type.Finder.FindBy(typeof(T).Namespace, "Version");
				private static Y GetField<Y>(string fieldName, Y defaultValue) => Reflection.GetField<Y>(Class, fieldName, defaultValue);

				public static string Namespace => GetField<string>("Namespace", typeof(T).Namespace);
				public static string Vendor => GetField<string>("Vendor", null);
				public static string FriendlyName => GetField<string>("FriendlyName", typeof(T).Name);

				public static int major => GetField<int>("major", 0);
				public static int minor => GetField<int>("minor", 0);
				public static int patch => GetField<int>("patch", 0);
				public static int build => GetField<int>("build", 0);
				public static System.Version V => new System.Version(major, minor, patch, build);
				public static string Number => GetField<string>("Number", "0.0.0.0");
				public static string Text => GetField<string>("Text", "0.0.0.0");
				public static string EffectivePath = null == Vendor ? Namespace : SIO.Path.Combine(Vendor, Namespace);
			}

			internal static class Configuration
			{
				internal static Util.KSP.Version deriveMax()
				{
					// Handles hypothetical future KSP versions.
					Util.KSP.Version max = Util.KSP.PUBLISHED_VERSIONS[Util.KSP.PUBLISHED_VERSIONS.Length - 1];
					return (max < Util.KSP.Version.Current) ? Util.KSP.Version.Current : max;
				}

				public static int[] Unity(SType klass) => GetField<int[]>(klass, "Unity", new int[0]);

				public static class KSP
				{
					public static KSPe.Util.KSP.Version Min(SType klass)	=> GetField<Util.KSP.Version>(
																						Reflection.GetClass(klass, "KSP")
																						,"Min"
																						, Util.KSP.Version.FindByVersion(0,0,0)
																					);
					public static KSPe.Util.KSP.Version Max(SType klass)	=> GetField<Util.KSP.Version>(
																						Reflection.GetClass(klass, "KSP")
																						,"Max"
																						, deriveMax()
																					);
				}

				public static class Dependencies
				{
					public static string[] Assemblies(SType klass)	=> GetField<string[]>(
																						Reflection.GetClass(klass, "Dependencies")
																						,"Assemblies"
																						, new string[0]
																					);
					public static string[] Types(SType klass)		=> GetField<string[]>(
																						Reflection.GetClass(klass, "Dependencies")
																						,"Types"
																						, new string[0]
																					);
				}

				public static class Conflicts
				{
					public static string[] Assemblies(SType klass)	=> GetField<string[]>(
																						Reflection.GetClass(klass, "Conflicts")
																						,"Assemblies"
																						, new string[0]
																					);
					public static string[] Types(SType klass)		=> GetField<string[]>(
																						Reflection.GetClass(klass, "Conflicts")
																						,"Types"
																						, new string[0]
																					);
				}

			}

			public static class Configuration<T>
			{
				public static SType Class = Type.Finder.ExistsBy(typeof(T).Namespace, "Configuration")
											? Type.Finder.FindBy(typeof(T).Namespace, "Configuration")
											: null
										;
				private static Y GetField<Y>(string fieldName, Y defaultValue) =>
																			Reflection.GetField<Y>(Class, fieldName, defaultValue);
				public static int[] Unity => GetField<int[]>("Unity", new int[0]);

				public static class KSP
				{
					public static SType Class = Configuration<T>.Class?.GetNestedType("KSP");
					private static Y GetField<Y>(string fieldName, Y defaultValue) =>
																			Reflection.GetField<Y>(Class, fieldName, defaultValue);
					public static KSPe.Util.KSP.Version	Min => GetField<KSPe.Util.KSP.Version>("Min", Util.KSP.Version.Current);
					public static KSPe.Util.KSP.Version	Max => GetField<KSPe.Util.KSP.Version>("Max", Configuration.deriveMax());
				}

				public static class Dependencies
				{
					public static SType Class = Configuration<T>.Class?.GetNestedType("Dependencies");
					private static Y GetField<Y>(string fieldName, Y defaultValue) =>
																			Reflection.GetField<Y>(Class, fieldName, defaultValue);
					public static string[] Assemblies	=> GetField<string[]>("Assemblies", new string[0]);
					public static string[] Types		=> GetField<string[]>("Types", new string[0]);
				}

				public static class Conflicts
				{
					public static SType Class = Configuration<T>.Class?.GetNestedType("Conflicts");
					private static Y GetField<Y>(string fieldName, Y defaultValue) =>
																			Reflection.GetField<Y>(Class, fieldName, defaultValue);
					public static string[] Assemblies	=> GetField<string[]>("Assemblies", new string[0]);
					public static string[] Types		=> GetField<string[]>("Types", new string[0]);
				}
			}
		}
	}
}

