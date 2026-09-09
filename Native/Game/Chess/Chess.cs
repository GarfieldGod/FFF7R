using System.Data.Common;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

public class Chess
{
    public Action OnDead { get; set; }
    public ChessProperty Property => property_;
    private ChessProperty property_;

    public Chess(ChessProperty property)
    {
        property_ = new ChessProperty(property);
    }
    public Chess Clone()
    {
        return new Chess(this.property_);
    }

    public void Dead()
    {
        OnDead?.Invoke();
    }
}