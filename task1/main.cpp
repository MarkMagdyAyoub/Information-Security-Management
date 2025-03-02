#include <iostream>
#include <filesystem>
#include <fstream>
#include <string>
#include <thread>
#include <functional>
#include <atomic>

#define PASSWORD7 "./dictionary/7-more-passwords.txt"
#define PASSWORD8 "./dictionary/8-more-passwords.txt"
#define REAL_PASSWORD "abc" // assume it is given from the database

using namespace std;

// i used this flag to early break all threads if i found the answer in one thread
// the shared resource should be atomic to avoid race condition
atomic<bool> passwordFound(false); // shared variable(flag)

bool check_password(const string &filePath, const string &password)
{
  if (!filesystem::exists(filesystem::path(filePath)))
    throw runtime_error("File not found");

  ifstream fin(filePath);
  string savedPassword;
  while (getline(fin, savedPassword))
  {
    if (passwordFound.load())
      return false;
    if (password == savedPassword)
    {
      fin.close();
      passwordFound.store(true);
      return true;
    }
  }
  fin.close();
  return false;
}

bool brute_force(int length)
{
  string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
  function<bool(string word, int n)> generate = [&](string word, int n)
  {
    if (passwordFound.load())
      return false;
    if (n == length)
    {
      if (word == REAL_PASSWORD)
      {
        passwordFound.store(true);
        return true;
      }
      return false;
    }
    for (char c : chars)
      if (generate(word + c, n + 1))
        return true;
    return false;
  };
  return generate("", 0);
}

/**
 * Creates two threads:
 * [1] One for searching in the dictionary files
 * [2] One for brute force
 */

int main()
{
  string username;
  bool searchFound = false, bfFound = false;

  getline(cin, username);

  // dictionary attack
  thread search(
      [&]()
      {
        searchFound = check_password(PASSWORD7, REAL_PASSWORD) || check_password(PASSWORD8, REAL_PASSWORD);
      });

  // brute force
  thread bf(
      [&]()
      {
        bfFound = brute_force(5); // 52^5 --> 380,204,032
      });

  search.join();
  bf.join();

  if (searchFound || bfFound)
    cout << "Password found" << endl;
  else
    cout << "Password not found" << endl;
  return 0;
}