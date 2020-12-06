using MouseSimulator.Models;

namespace MouseSimulator.Interfaces
{
    public interface ISimulator
    {
        void MoveMouseBy(int pixelDeltaX, int pixelDeltaY);
        void MouseLeftDown();
        void MouseLeftUp();
        void KeyPress(params VirtualKeyCode[] keyCodes);
    }
}
