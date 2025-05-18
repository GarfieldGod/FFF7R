using System.Data.Common;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

public class Chess
{
    private ChessProperty property_;
    private string cardCode_;
    private string chessName_;
    private int level_;
    private int cost_;
    private string id_;
    private Float3D chessPos_ = new Float3D(0, 0, 0);
    public Chess(ChessProperty property)
    {
        property_ = new ChessProperty(property);
        cardCode_ = property.CardCode;
        chessName_ = property.Name;
        level_ = property.Level;
        cost_ = property.Cost;
    }
    public Chess Clone()
    {
        return new Chess(this.property_);
    }
    public ChessProperty GetChessProperty() {
        return property_;
    }
    public void SetPos(Float3D chessPos) {
        chessPos_ = chessPos;
    }
}