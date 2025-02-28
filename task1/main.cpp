#include <iostream>
#include <filesystem>
#include <fstream>
#include <string>
#include <thread>
#define PASSWORD7 "./dictionary/7-more-passwords.txt"
#define PASSWORD8 "./dictionary/8-more-passwords.txt"
#define REAL_PASSWORD "Leonard1" // assume it is given from the database
using namespace std;

bool check_password(const string &filePath, const string &password)
{
  if (!filesystem::exists(filesystem::path(filePath)))
    throw runtime_error("File not found");

  ifstream fin(filePath);
  string savedPassword;
  while (getline(fin, savedPassword))
  {
    if (password == savedPassword)
    {
      fin.close();
      return true;
    }
  }
  fin.close();
  return false;
}

bool brute_force()
{
  char characters[] = {
      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
      'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

  int size = sizeof(characters) / sizeof(characters[0]);

  for (int i = 0; i < size; i++)
  {
    for (int j = 0; j < size; j++)
    {
      for (int k = 0; k < size; k++)
      {
        for (int l = 0; l < size; l++)
        {
          for (int m = 0; m < size; m++)
          {
            string result = string(1, characters[i]) + characters[j] + characters[k] + characters[l] + characters[m];
            if (result == REAL_PASSWORD)
              return true;
          }
        }
      }
    }
  }
  return false;
}

/**
 * Creates two threads:
 * - One for searching in the dictionary files
 * - One for brute force
 */

int main()
{
  // dictionary attack
  string username;
  bool searchFound = false, bfFound = false;

  getline(cin, username);

  thread search(
      [&]()
      {
        searchFound = check_password(PASSWORD7, REAL_PASSWORD) || check_password(PASSWORD8, REAL_PASSWORD);
      });

  thread bf(
      [&]()
      {
        bfFound = brute_force();
      });

  search.join();
  bf.join();

  if (searchFound || bfFound)
    cout << "Password found" << endl;
  else
    cout << "Password not found" << endl;
  return 0;
}