using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KS.Reactor;

// Minimap UI
public class Minimap : MonoBehaviour
{
    public RectTransform Cells;
    public RectTransform VerticalLines;
    public RectTransform HorizontalLines;
    public RectTransform Indicators;
    public RectTransform PlayerIcon;

    private int m_gridSize;

    public void CreateGrid(int size)
    {
        if (size <= 0)
        {
            return;
        }
        gameObject.SetActive(true);
        m_gridSize = size;
        float mapSize = ((RectTransform)transform).rect.width;
        float cellSize = mapSize / size;
        for (int i = Cells.childCount - 1; i >= 1; i--)
        {
            Destroy(Cells.GetChild(i).gameObject);
        }
        RectTransform template = (RectTransform)Cells.GetChild(0);
        template.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize);
        template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize);
        Image image = template.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, image.color.a);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                RectTransform cell = Instantiate(template, Cells);
                Vector3 position = cell.localPosition;
                position.x += cellSize * x;
                position.y += cellSize * y;
                cell.localPosition = position;
            }
        }

        if (size == 1)
        {
            VerticalLines.gameObject.SetActive(false);
            HorizontalLines.gameObject.SetActive(false);
        }
        else
        {
            VerticalLines.gameObject.SetActive(true);
            HorizontalLines.gameObject.SetActive(true);
            int numLines = size - 1;
            if (VerticalLines.childCount != numLines)
            {
                for (int i = Indicators.childCount - 1; i >= numLines; i--)
                {
                    Destroy(VerticalLines.GetChild(i).gameObject);
                    Destroy(HorizontalLines.GetChild(i).gameObject);
                }
                RectTransform vtemplate = (RectTransform)VerticalLines.GetChild(0);
                RectTransform htemplate = (RectTransform)HorizontalLines.GetChild(0);
                for (int i = 0; i < numLines; i++)
                {
                    RectTransform line = i < VerticalLines.childCount ? 
                        (RectTransform)VerticalLines.GetChild(i) : Instantiate(vtemplate, VerticalLines);
                    Vector2 position = line.anchoredPosition;
                    position.x = cellSize * (i + 1);
                    line.anchoredPosition = position;

                    line = i < HorizontalLines.childCount ?
                        (RectTransform)HorizontalLines.GetChild(i) : Instantiate(htemplate, HorizontalLines);
                    position = line.anchoredPosition;
                    position.y = cellSize * (i + 1);
                    line.anchoredPosition = position;
                }
            }
        }
    }

    // Converts world coordinates to grid coordinates and sets the color of the tile in the map at those coordinates.
    public void SetColor(Vector3 position, Color color)
    {
        position += new Vector3(World.TILE_SIZE / 2f, 0f, World.TILE_SIZE / 2f);
        int x = (int)(position.x / World.TILE_SIZE);
        int y = (int)(position.z / World.TILE_SIZE);
        SetColor(x, y, color);
    }

    // Sets the color of a tile in the map.
    public void SetColor(int x, int y, Color color)
    {
        int index = x * m_gridSize + y;
        if (index < Cells.childCount)
        {
            Image image = Cells.GetChild(index).GetComponent<Image>();
            color.a = image.color.a;
            image.color = color;
        }
    }

    // Clears the color of all tiles and removes all indicators on the map.
    public void Clear()
    {
        foreach (Transform cell in Cells)
        {
            Image image = cell.GetComponent<Image>();
            Color color = Color.black;
            color.a = image.color.a;
            image.color = color;
        }
        for (int i = Indicators.childCount - 1; i >= 1; i--)
        {
            Destroy(Indicators.GetChild(i).gameObject);
        }
    }

    // Creates an icon to represent a player's location.
    public RectTransform CreatePlayerIcon()
    {
        if (PlayerIcon == null)
        {
            return null;
        }
        GameObject icon = Instantiate(PlayerIcon.gameObject, Indicators);
        icon.SetActive(true);
        return (RectTransform)icon.transform;
    }

    // Sets the position and rotation of an icon on the map. Position is in world coordinates.
    public void SetPosition(RectTransform icon, Vector3 position, float rotation)
    {
        if (m_gridSize <= 0f || icon == null)
        {
            return;
        }
        float mapSize = ((RectTransform)transform).rect.width;
        Vector2 pos = new Vector2(position.x, position.z);
        pos += new Vector2(World.TILE_SIZE / 2f, World.TILE_SIZE / 2f);
        pos *= mapSize / (World.TILE_SIZE * m_gridSize);
        icon.anchoredPosition = pos;
        icon.rotation = Quaternion.Euler(0f, 0f, -rotation + 90f);
    }
}
