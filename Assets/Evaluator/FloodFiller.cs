using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FloodFiller
{
    public FloodFiller(int width, int height)
    {
        visited = new bool[width, height];
        for (int iy = 0; iy < height; iy++) {
            for (int ix = 0; ix < width; ix++) {
                visited[ix, iy] = false;
            }
        }
    }

    public delegate void on_start(int ix, int iy);
    public void start_flood_fill(int x, int y, on_start predicate, on_check fill_check, on_fill if_check_true, bool do_diagonal = false)
    {
        if (visited[x, y]) {
            return;
        }

        predicate(x, y);
        flood_fill(x, y, fill_check, if_check_true, do_diagonal);
    }

    public delegate bool on_check(int ix, int iy);
    public delegate void on_fill(int ix, int iy);
    private void flood_fill(int x, int y, on_check fill_check, on_fill if_check_true, bool do_diagonal)
    {
        if (x < 0 || y < 0 ||
           x > visited.GetLength(0) - 1 || y > visited.GetLength(1) - 1) {
            return;
        }

        if (visited[x, y]) {
            return;
        }

        visited[x, y] = true;
        if (fill_check(x, y)) {
            if_check_true(x, y);
            flood_fill(x + 1, y + 0, fill_check, if_check_true, do_diagonal);
            flood_fill(x - 1, y + 0, fill_check, if_check_true, do_diagonal);
            flood_fill(x + 0, y + 1, fill_check, if_check_true, do_diagonal);
            flood_fill(x + 0, y - 1, fill_check, if_check_true, do_diagonal);

            if (do_diagonal) {
                flood_fill(x + 1, y + 1, fill_check, if_check_true, do_diagonal);
                flood_fill(x - 1, y + 1, fill_check, if_check_true, do_diagonal);
                flood_fill(x - 1, y - 1, fill_check, if_check_true, do_diagonal);
                flood_fill(x + 1, y - 1, fill_check, if_check_true, do_diagonal);
            }
        }
    }

    public void reset()
    {
        for (int iy = 0; iy < visited.GetLength(0); iy++) {
            for (int ix = 0; ix < visited.GetLength(1); ix++) {
                visited[ix, iy] = false;
            }
        }
    }

    private bool[,] visited;
}