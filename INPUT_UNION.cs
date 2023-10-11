using System.Runtime.InteropServices;

namespace ConToJREts
{
  [StructLayout(LayoutKind.Explicit)]
  public struct INPUT_UNION
  {
    [FieldOffset(0)]
    public MOUSEINPUT mouse;
    [FieldOffset(0)]
    public KEYBDINPUT keyboard;
    [FieldOffset(0)]
    public HARDWAREINPUT hardware;
  }
}
