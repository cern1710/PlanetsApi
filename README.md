# PlanetsApi

A very simple C# API that retrieves data from the Star Wars API.

## User Guide

- GET a list of planets: GET `https://localhost:7231/planets`.
- GET a list of favorite planets: GET `https://localhost:7231/favorites`.
- POST a favorite planet: POST `https://localhost:7231/favorites/{planet name}`.
- DELETE a favorite planet: DELETE `https://localhost:7231/favorites/{planet name}`.
- GET a random planet not in your favorites: GET on `https://localhost:7231/random`.
