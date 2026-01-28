using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "PlaceRepository", menuName = "Data/PlaceRepository")]
public class PlaceRepository : ScriptableObject
{
    [SerializeField] private List<PlaceDataSO> m_places = new();
    [SerializeField] private List<CountryDataSO> m_countries = new();
    // 에디터에서 데이터를 주입하거나 런타임에 참조할 때 사용
    public List<PlaceDataSO> Places
    {
        get => m_places;
        set => m_places = value;
    }

    public List<CountryDataSO> Countries
    {
        get => m_countries;
        set => m_countries = value;
    }



    /// <summary>
    /// Place ID를 통해 특정 장소 데이터를 찾습니다.
    /// </summary>
    public PlaceDataSO GetPlaceByPlaceID(string id)
    {
        return m_places.FirstOrDefault(m => m != null && m.placeID == id);
    }

    /// <summary>
    /// Country ID를 통해 특정 국가 데이터를 찾습니다.
    /// </summary>
    public CountryDataSO GetCountryByCountryID(string id)
    {
        // 주의: m_countries 리스트에서 찾아야 하므로 수정되었습니다.
        return m_countries.FirstOrDefault(m => m != null && m.countryID == id);
    }

    /// <summary>
    /// 모든 장소 ID 목록을 가져옵니다.
    /// </summary>
    public List<string> GetPlaceIDs()
    {
        return m_places
            .Where(m => m != null)
            .Select(m => m.placeID)
            .ToList();
    }

    /// <summary>
    /// 모든 국가 ID 목록을 가져옵니다.
    /// </summary>
    public List<string> GetCountryIDs()
    {
        return m_countries
            .Where(m => m != null)
            .Select(m => m.countryID)
            .ToList();
    }

    /// <summary>
    /// 플레이스  id 로부터 국가 데이터를 찾습니다.
    /// </summary>
    /// <param name="placeData"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public CountryDataSO GetCountryByPlaceID(string placeIds)
    {
        return m_countries.FirstOrDefault(country =>
            country != null && country.countryID == GetPlaceByPlaceID(placeIds)?.countryID);
    }

    /// <summary>
    /// 매개 변수로 받은 리스트에 해당하는 나라 데이터 리스트를 반환합니다.
    /// </summary>
    public List<CountryDataSO> GetCountriesByIDs(List<string> countryIDs)
    {
        return m_countries
            .Where(country => country != null && countryIDs.Contains(country.countryID))
            .ToList();
    }
}