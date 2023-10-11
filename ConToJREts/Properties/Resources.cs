// Decompiled with JetBrains decompiler
// Type: ConToJREts.Properties.Resources
// Assembly: ConToJREts, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0E80859E-6178-44C6-95E3-9BCEE5E61554
// Assembly location: C:\Users\cracrayol\Desktop\ConToJREts\ConToJREts.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ConToJREts.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (ConToJREts.Properties.Resources.resourceMan == null)
          ConToJREts.Properties.Resources.resourceMan = new ResourceManager("ConToJREts.Properties.Resources", typeof (ConToJREts.Properties.Resources).Assembly);
        return ConToJREts.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => ConToJREts.Properties.Resources.resourceCulture;
      set => ConToJREts.Properties.Resources.resourceCulture = value;
    }
  }
}
