public class ChessPadView
{
    private readonly ChessPad _sourcePad;
    private readonly bool _isFlipped;

    public ChessPadView(ChessPad sourcePad, bool isFlipped)
    {
        _sourcePad = sourcePad;
        _isFlipped = isFlipped;
    }

    public PadGrid this[int viewX, int viewY]
    {
        get
        {
            Int2D realPos = ViewToReal(viewX, viewY);
            return _sourcePad[realPos.x, realPos.y];
        }
    }

    private Int2D ViewToReal(int vx, int vy)
    {
        if (!_isFlipped) return new Int2D(vx, vy);
        int realX = _sourcePad.Height - 1 - vx;
        int realY = _sourcePad.Width -1 - vy;
        return new Int2D(realX, realY);
    }
}