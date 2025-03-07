#include <iostream>
#include <vector>
using namespace std;

static const vector<vector<int>> posEffects = {
    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 2, 0, 0, 0 },
    { 0, 0, 0, -2, 9, 1, -1, 0, 0 },
    { 0, 0, 0, 0, 1, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
};

static const vector<vector<string>> chessPos = {
    { "upLine0"  , "upLine1"  , "upLine2"  , "upLine3"  , "upLine4"   },
    { "midLine0" , "midLine1" , "midLine2" , "midLine3" , "midLine4"  },
    { "downLine0", "downLine1", "downLine2", "downLine3", "downLine4" }
};

vector<pair<pair<int,int>,int>> ParseEffects(vector<vector<int>> effects) {
    pair<int,int> midPos((effects.size()-1)/2, (effects[0].size()-1)/2);
    vector<pair<pair<int,int>,int>> result;
    for (int i = 0; i < effects.size(); i++) {
        for (int j = 0; j < effects[i].size(); j++) {
            if (effects[i][j] != 0) {
                int posY = i - midPos.first;
                int posX = j - midPos.second;
                if ( posX != 0 || posY !=0 ) {
                    pair<pair<int,int>,int> posWithValue(make_pair(posY, posX), effects[i][j]);
                    result.push_back(posWithValue);
                    printf("y: %d x: %d value: %d", posY, posX, effects[i][j]);
                }
            }
        }
        printf("\n");
    }
    return result;
}

bool ParseEffectsInPosition(string posName, vector<pair<pair<int,int>,int>> parsedEffects) {
    pair<int, int> posPos(-1, -1);
    for (int i = 0; i < chessPos.size(); i++) {
        for (int j = 0; j < chessPos[i].size(); j++) {
            if(chessPos[i][j] == posName) {
                posPos.first = i;
                posPos.second = j;
            }
        }
    }
    if (posPos.first < 0 || posPos.second < 0) {
        return false;
    }
    printf("posPos: %d %d\n", posPos.first, posPos.second);
    for (const auto &effect : parsedEffects) {
        int posY = posPos.first + effect.first.first;
        int posX = posPos.second + effect.first.second;
        if (posY < chessPos.size() && posY >= 0 && posX < chessPos[0].size() && posX >= 0)
        {
            pair<int, int> affectedPos(posY, posX);
            printf("affectedPos: %s \tpos: (%d, %d) effect: %d\n", chessPos[posY][posX].c_str(), affectedPos.first, affectedPos.second, effect.second);
        }
    }
    return true;
}

int main() {
    string posName = "midLine1";
    auto parsedEffects = ParseEffects(posEffects);
    bool parseSucc = ParseEffectsInPosition(posName, parsedEffects);
    return 0;
}