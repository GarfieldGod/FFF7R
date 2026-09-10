using System.Data.Common;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using Effect;

public class Chess
{
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
}